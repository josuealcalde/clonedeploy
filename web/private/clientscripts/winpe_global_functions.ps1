$clientLog="x:\clientlog.log"

function log($message, $isDisplay)
{
	Add-Content $clientLog ""
	if($isDisplay -eq "true")
    {
        Write-Host $message | tee -Append -FilePath $clientLog
    }
    Add-Content $clientLog $message
}

function Checkout()
{
    log " ** Closing Active Task ** " "true"
    $computerIdBytes = [System.Text.Encoding]::UTF8.GetBytes($computer_id)
    $computerIdEncoded =[Convert]::ToBase64String($computerIdBytes)
    (Get-Content $clientLog) -replace ("`0","") | Set-Content $clientLog
	$logBytes = [System.IO.File]::ReadAllBytes("$clientLog")
    $logEncoded =[Convert]::ToBase64String($logBytes)
    $imageDirectionBytes = [System.Text.Encoding]::UTF8.GetBytes($image_direction)
    $imageDirectionEncoded =[Convert]::ToBase64String($imageDirectionBytes)
    $macBytes = [System.Text.Encoding]::UTF8.GetBytes($script:mac)
    $macEncoded =[Convert]::ToBase64String($macBytes)
    curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded -F computerId="$computerIdEncoded" -F logContents="$logEncoded" -F subType="$imageDirectionEncoded" -F mac="$macEncoded" "${script:web}UploadLog" --connect-timeout 10 --stderr -

    if($multicast -eq "true")
    {
        curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded --data "portBase=$multicast_port" "${script:web}MulticastCheckOut" --connect-timeout 10 --stderr -
	}

    if(!$script:isOnDemand)
    {
        if(!$script:isPermanentTask)
        {
            curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded --data "computerId=$computer_id" "${script:web}CheckOut" --connect-timeout 10 --stderr -
        }
        else
        {
            curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded --data "computerId=$computer_id" "${script:web}PermanentTaskCheckOut" --connect-timeout 10 --stderr -
        }
    }

    if($task_completed_action.trim("`"") -eq "Power Off")
    {
		wpeutil shutdown
    }

	elseif($task_completed_action.trim("`"") -eq "Exit To Shell")
    {
		[Environment]::Exit(0)
    }
	else
    {
		wpeutil reboot
	} 
}

function error($message, $rebootTime)
{
	Write-Host
	log -message " ** An Error Has Occurred ** " -isDisplay "true"
	log -message " ...... $message" -isDisplay "true"
	Write-Host
	Write-Host " ** Rebooting In One Minute ** "
	
	if(!$computer_id)
    {
	  $computer_id="-1"
	}
    else
    {
        curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded --data "computerId=$computer_id&error=$1" ${script:web}ErrorEmail  --connect-timeout 10 --stderr -
	}

    $computerIdBytes = [System.Text.Encoding]::UTF8.GetBytes($computer_id)
    $computerIdEncoded =[Convert]::ToBase64String($computerIdBytes)
    (Get-Content $clientLog) -replace ("`0","") | Set-Content $clientLog
	$logBytes = [System.IO.File]::ReadAllBytes("$clientLog")
    $logEncoded =[Convert]::ToBase64String($logBytes)
    $imageDirectionBytes = [System.Text.Encoding]::UTF8.GetBytes($image_direction)
    $imageDirectionEncoded =[Convert]::ToBase64String($imageDirectionBytes)
    $macBytes = [System.Text.Encoding]::UTF8.GetBytes($script:mac)
    $macEncoded =[Convert]::ToBase64String($macBytes)
    curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded -F computerId="$computerIdEncoded" -F logContents="$logEncoded" -F subType="$imageDirectionEncoded" -F mac="$macEncoded" "${script:web}UploadLog" --connect-timeout 10 --stderr -
	
	if($rebootTime)
    {
	  Start-Sleep -s $rebootTime
    }
	else
    {
	  Start-Sleep -s 60
	}
	if($task_completed_action.trim("`"") -eq "Power Off")
    {
		shutdown
    }

	elseif($task_completed_action.trim("`"") -eq "Exit To Shell")
    {
		[Environment]::Exit(0)
    }
	else
    {
		reboot

	} 
}

function Mount-SMB()
{
    log " ** Mounting SMB Share ** " "true"
	
	$smbInfo=$(curl.exe $script:curlOptions -H Authorization:$script:userTokenEncoded --data "dpId=$dp_id&task=$image_direction" ${script:web}DistributionPoint  --connect-timeout 10 --stderr -)
	$smbInfo=$smbInfo | ConvertFrom-Json
    if(!$?)
    {
        $Error[0].Exception.Message
        $smbInfo
        error "Could Not Parse SMB Info"
    }
	#fix path that was originally only used for initrd
	$sharePath=$smbInfo.SharePath -replace ("/"),("\")
    net use s: /Delete /Yes 2>&1 >> $null
    Start-Sleep 2
    net use s: $sharePath /user:$($smbInfo.Domain)\$($smbInfo.Username) $smbInfo.Password 2>x:\mntstat >> $clientLog
    
	if(!$?)
    {
	    Get-Content x:\mntstat | Out-File $clientLog -Append
		error -message "Could Not Mount SMB Share: $(Get-Content x:\mntstat)"		
    }
	else
    {
        Start-Sleep 2
	    log -message " ...... Success" -isDisplay "true"
        Write-Host
		cd s:\images\$img_name
	    if(!$?)
        {
	        error "Could Not Change Directory To s:\images\$image_name Verify The Directory Exists And Permissions Are Correct"
	    }
    }
	Start-Sleep 2
}

function Get-Hard-Drives($taskType)
{
    log " ** Looking For Hard Drive(s) **" "true"
    log " ** Displaying Available Devices ** "
    Get-Disk | Out-File $clientLog -Append
    if($custom_hard_drives)
    {
        if($custom_hard_drives.trim("`""))
        {
            #Todo - finish custom hard drives
            log " ...... Hard Drive(s) Set By Image Profile: $hard_drives"
        }
        else
        {
            if($taskType -eq "upload")
            {
                $script:HardDrives=$(get-disk | where-object {$_.NumberOfPartitions -gt 0 -and $_.BusType -ne "USB"} | Sort-Object Number)
            }
            else
            {
                $script:HardDrives=$(get-disk | where-object {$_.BusType -ne "USB"} | Sort-Object Number)
            }
        }
    }
    else
    {
        if($taskType -eq "upload")
        {
            $script:HardDrives=$(get-disk | where-object {$_.NumberOfPartitions -gt 0 -and $_.BusType -ne "USB"} | Sort-Object Number)
        }
        else
        {
            $script:HardDrives=$(get-disk | where-object {$_.BusType -ne "USB"} | Sort-Object Number)
        }
    }

    if(@($script:HardDrives).count -eq 0)
    {
        error "Could Not Find A Hard Drive Attached To This Computer."
    }

    log " ...... Found $(@($script:HardDrives).count) Drives" "true"

    foreach($hardDrive in $script:HardDrives)
    {
        log " ** Displaying Current Partition Table On $($hardDrive.Number)"
        Get-Partition -DiskNumber $hardDrive.Number | Out-File $clientLog -Append
    }
    Write-Host
}


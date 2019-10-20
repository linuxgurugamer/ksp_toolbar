#!/bin/bash

IFS=$'\n'


function ReplaceReferences()
{
	file=$1
newRef='     <Reference Include="$(KSPDIR)\KSP_x64_Data\Managed\Assembly*.dll">
	   <Private>False</Private>
    </Reference>
    <Reference Include="$(KSPDIR)\KSP_x64_Data\Managed\UnityEngine*.dll">
	   <Private>False</Private>
    </Reference> '
ref=""
first=false
while IFS= read -r line; do
	if [ "$ref" = "" ]; then
		if [[ $line == *"<Reference"* ]]; then
			ref=$line
		else
			echo $line
		fi
	else
		ref=${ref}$'\n'${line}
		if [[ $line = *"</Reference"* ]]; then
			if [[ "$ref" = *"R:\\KSP_1.7.3_dev\\KSP_x64_Data"* ]]; then
				ref=""
				[ $first = false ] && echo "$newRef"
				first=true
			else
				echo "$ref"
				ref=""
			fi
		fi
	fi
done <$file
	
}

function ReplaceKspDir()
{
	ReplaceReferences $1 | sed 's/R:\\KSP_1.7.3_dev/\$(KSPDIR)/g'
}

#file=Toolbar.csproj
#ReplaceKspDir $file
#exit

for v in 1.7.3; do
	find . -type f -name '*.csproj' -print0 | 
	while IFS= read -r -d '' file; do
	

		[ ! -f ${file}.173 ] && cp $file ${file}.173

		ReplaceKspDir ${file} > ${file}.new
		if [ ! -f ${file}.orig ]; then
			mv ${file} ${file}.orig
		else
			rm ${file}
		fi
		mv ${file}.new ${file}

	done
	find . -type f -name 'deploy.bat' -print0 | 
	while IFS= read -r -d '' file; do
		sed -i 's/R:\\KSP_1.7.3_dev/$(KSPDIR)/g' $file
		#sed -i "s/$v/1.7.3/g" $file
	done
done





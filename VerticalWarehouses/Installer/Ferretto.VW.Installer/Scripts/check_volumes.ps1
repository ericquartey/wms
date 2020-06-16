if((Get-ChildItem D: | Measure-Object).Count -eq 0)
{
    echo "Folder D: is empty"
}
else
{
    throw "Folder is not empty"
}

if((Get-ChildItem E: | Measure-Object).Count -eq 0)
{
    echo "Folder E: is empty"
}
else
{
    throw "Folder is not empty"
}

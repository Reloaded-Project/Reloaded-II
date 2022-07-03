namespace Reloaded.Community.Tool;

public static class Samples
{
    public static readonly AppItem AppItem = new AppItem()
    {
        AppId = "application.id",
        AppName = "Application Name",
        AppStatus = Status.Ok,
        BadStatusDescription = "Application Description",
        GameBananaId = 1337,
        Hash = "XXHASH64 of EXE Goes Here",
        Warnings = new List<WarningItem>() 
        {
            new WarningItem()
            {
                ErrorMessage = "Sample Error",
                Items = new List<VerifyItem>()
                {
                    new VerifyItem()
                    {
                        Hash = "XXHASH64 Goes Here",
                        FilePath = "legit_folder/legit_file.zip"
                    }
                }
            }
        }
    };
}
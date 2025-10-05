// BlackBook/Services/Services.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Service Locator (Dialogs)
namespace BlackBook.Services;

public static class Services {
    public static readonly BlackBook.Services.Dialogs.IDialogService Dialogs = new BlackBook.Services.Dialogs.DialogService();
}

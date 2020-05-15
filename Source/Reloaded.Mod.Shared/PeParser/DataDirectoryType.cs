namespace Reloaded.Mod.Shared.PeParser
{
    public enum DataDirectoryType
    {
        ExportTable = 0,
        ImportTable,
        ResourceTable,
        ExceptionTable,

        CertificateTable,
        BaseRelocationTable,
        Debug,
        Architecture,
        
        GlobalPtr,
        TLSTable,
        LoadConfigTable,
        BoundImport,
        
        IAT,
        DelayImportDescriptor,
        CLRRuntimeHeader,
        Reserved,
    }
}

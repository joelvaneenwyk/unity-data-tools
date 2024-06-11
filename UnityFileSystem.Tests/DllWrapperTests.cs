using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UnityDataTools.TestCommon;

namespace UnityDataTools.FileSystem.Tests;

#pragma warning disable NUnit2005, NUnit2006

public class DllInitCleanupTests
{
    [TearDown]
    public void TearDown()
    {
        // In case a test fails
        DllWrapper.Cleanup();
    }

    [Test]
    public void Init_CalledOnce_ReturnSuccess()
    {
        var r = DllWrapper.Init();
        ClassicAssert.AreEqual(ReturnCode.Success, r);
    }

    [Test]
    public void Cleanup_CalledOnce_ReturnSuccess()
    {
        DllWrapper.Init();
        var r = DllWrapper.Cleanup();
        ClassicAssert.AreEqual(ReturnCode.Success, r);
    }

    [Test]
    public void Init_CalledTwice_ReturnError()
    {
        DllWrapper.Init();
        var r = DllWrapper.Init();
        ClassicAssert.AreEqual(ReturnCode.AlreadyInitialized, r);
    }

    [Test]
    public void Cleanup_CalledTwice_ReturnError()
    {
        DllWrapper.Init();

        DllWrapper.Cleanup();
        var r = DllWrapper.Cleanup();
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);
    }

    [Test]
    public void AnyFunction_NotInitialized_ReturnError()
    {
        var r = DllWrapper.MountArchive("", "", out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.UnmountArchive(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetArchiveNodeCount(new UnityArchiveHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetArchiveNode(new UnityArchiveHandle(), 0, null, 0, out _, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.OpenFile("", out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.ReadFile(new UnityFileHandle(), 0, null, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.SeekFile(new UnityFileHandle(), 0, 0, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetFileSize(new UnityFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.CloseFile(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.OpenSerializedFile("", out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.CloseSerializedFile(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetExternalReferenceCount(new SerializedFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetExternalReference(new SerializedFileHandle(), 0, null, 0, null, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetObjectCount(new SerializedFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetObjectInfo(new SerializedFileHandle(), null, 0);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetTypeTree(new SerializedFileHandle(), 0, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);

        r = DllWrapper.GetTypeTreeNodeInfo(new TypeTreeHandle(), 0, null, 0, null, 0, out _, out _, out _, out _, out _, out _);
        ClassicAssert.AreEqual(ReturnCode.NotInitialized, r);
    }
}

public class DllMountUnmountTests : AssetBundleTestFixture
{
    public DllMountUnmountTests(Context context) : base(context)
    {
    }
    
    [OneTimeSetUp]
    public void Setup()
    {
        DllWrapper.Init();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        DllWrapper.Cleanup();
    }

    [Test]
    public void MountArchive_InvalidPath_ReturnError()
    {
        var r = DllWrapper.MountArchive("bad/path", "archive:/", out var handle);
        ClassicAssert.AreEqual(ReturnCode.FileNotFound, r);
        ClassicAssert.True(handle.IsInvalid);
    }

    [Test]
    public void MountArchive_InvalidArchive_ReturnError()
    {
        var path = Path.Combine(Context.TestDataFolder, "invalidfile");
        var r = DllWrapper.MountArchive(path, "archive:/", out var handle);
        ClassicAssert.AreEqual(ReturnCode.FileFormatError, r);
        ClassicAssert.True(handle.IsInvalid);
    }

    [Test]
    public void UnmountArchive_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.UnmountArchive(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void MountArchive_ActualArchive_ReturnSuccess()
    {
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        var r = DllWrapper.MountArchive(path, "archive:/", out var handle);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.IsFalse(handle.IsInvalid);

        handle.Dispose();
    }

    [Test]
    public void UnmountArchive_ActualArchive_ReturnSuccess()
    {
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        DllWrapper.MountArchive(path, "archive:/", out var handle);

        Assert.DoesNotThrow(() => handle.Dispose());
    }
}

public class DllArchiveTests : AssetBundleTestFixture
{
    private UnityArchiveHandle m_Archive;

    public DllArchiveTests(Context context) : base(context)
    {
    }

    [OneTimeSetUp]
    public void Setup()
    {
        DllWrapper.Init();
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        DllWrapper.MountArchive(path, "archive:/", out m_Archive);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        m_Archive.Dispose();
        DllWrapper.Cleanup();
    }

    [Test]
    public void GetArchiveNodeCount_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetArchiveNodeCount(new UnityArchiveHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetArchiveNodeCount_ReturnExpectedCount()
    {
        var r = DllWrapper.GetArchiveNodeCount(m_Archive, out var count);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("NodeCount"), count);
    }

    [Test]
    public void GetArchiveNode_ValidArchive_ReturnExpectedNode()
    {
        var path = new StringBuilder(256);
        var r = DllWrapper.GetArchiveNode(m_Archive, 0, path, 256, out var size, out var flags);
        ClassicAssert.AreEqual(ReturnCode.Success, r);

        ClassicAssert.AreEqual("CAB-5d40f7cad7c871cf2ad2af19ac542994", path.ToString());
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-Size"), size);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-Flags"), (long)flags);

        r = DllWrapper.GetArchiveNode(m_Archive, 1, path, 256, out size, out flags);
        ClassicAssert.AreEqual(ReturnCode.Success, r);

        ClassicAssert.AreEqual("CAB-5d40f7cad7c871cf2ad2af19ac542994.resS", path.ToString());
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994.resS-Size"), size);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994.resS-Flags"), (long)flags);

        r = DllWrapper.GetArchiveNode(m_Archive, 2, path, 256, out size, out flags);
        ClassicAssert.AreEqual(ReturnCode.Success, r);

        ClassicAssert.AreEqual("CAB-5d40f7cad7c871cf2ad2af19ac542994.resource", path.ToString());
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994.resource-Size"), size);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994.resource-Flags"), (long)flags);
    }

    [Test]
    public void GetArchiveNode_StringTooSmall_ReturnError()
    {
        var path = new StringBuilder(10);
        var r = DllWrapper.GetArchiveNode(m_Archive, 0, path, 10, out var size, out var flags);
        ClassicAssert.AreEqual(ReturnCode.DestinationBufferTooSmall, r);
    }

    [Test]
    public void GetArchiveNode_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetArchiveNode(new UnityArchiveHandle(), 0, new StringBuilder(), 256, out var size, out var flags);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }
}

public class DllLFileTests : AssetBundleTestFixture
{
    private UnityArchiveHandle m_Archive;

    public DllLFileTests(Context context) : base(context)
    {
    }

    [OneTimeSetUp]
    public void Setup()
    {
        DllWrapper.Init();
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        DllWrapper.MountArchive(path, "archive:/", out m_Archive);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        m_Archive.Dispose();
        DllWrapper.Cleanup();
    }

    [Test]
    public void OpenFile_InvalidPath_ReturnError()
    {
        var r = DllWrapper.OpenFile("bad/path", out _);
        ClassicAssert.AreEqual(ReturnCode.FileNotFound, r);
    }

    [Test]
    public void OpenFile_LocalFileSystem_ReturnSuccess()
    {
        var path = Path.Combine(Context.TestDataFolder, "TextFile.txt");
        var r = DllWrapper.OpenFile(path, out var file);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.IsFalse(file.IsInvalid);

        file.Dispose();
    }

    [Test]
    public void CloseFile_LocalFileSystem_ReturnSuccess()
    {
        var path = Path.Combine(Context.TestDataFolder, "TextFile.txt");
        DllWrapper.OpenFile(path, out var file);

        Assert.DoesNotThrow(() => file.Dispose());
    }

    [Test]
    public void CloseFile_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.CloseFile(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetFileSize_LocalFileSystem_ReturnSize()
    {
        var path = Path.Combine(Context.TestDataFolder, "TextFile.txt");
        DllWrapper.OpenFile(path, out var file);
        DllWrapper.GetFileSize(file, out var size);

        ClassicAssert.AreEqual(21, size);

        file.Dispose();
    }

    [Test]
    public void GetFileSize_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetFileSize(new UnityFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void SeekFile_LocalFileSystem_SeekAtExpectedPosition()
    {
        var path = Path.Combine(Context.TestDataFolder, "TextFile.txt");
        DllWrapper.OpenFile(path, out var file);

        DllWrapper.SeekFile(file, 16, SeekOrigin.Begin, out var newPos);
        ClassicAssert.AreEqual(16, newPos);

        var buffer = new Byte[16];
        DllWrapper.ReadFile(file, 4, buffer, out var actualSize);
        ClassicAssert.AreEqual(4, actualSize);
        ClassicAssert.AreEqual("file", Encoding.Default.GetString(buffer, 0, (int)actualSize));

        DllWrapper.SeekFile(file, -4, SeekOrigin.Current, out newPos);
        ClassicAssert.AreEqual(16, newPos);

        buffer = new Byte[16];
        DllWrapper.ReadFile(file, 4, buffer, out actualSize);
        ClassicAssert.AreEqual(4, actualSize);
        ClassicAssert.AreEqual("file", Encoding.Default.GetString(buffer, 0, (int)actualSize));

        DllWrapper.SeekFile(file, -5, SeekOrigin.End, out newPos);
        ClassicAssert.AreEqual(16, newPos);

        buffer = new Byte[16];
        DllWrapper.ReadFile(file, 4, buffer, out actualSize);
        ClassicAssert.AreEqual(4, actualSize);
        ClassicAssert.AreEqual("file", Encoding.Default.GetString(buffer, 0, (int)actualSize));

        file.Dispose();
    }

    [Test]
    public void SeekFile_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.SeekFile(new UnityFileHandle(), 0, SeekOrigin.Begin, out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void ReadFile_LocalFileSystem_ReadExpectedData()
    {
        var path = Path.Combine(Context.TestDataFolder, "TextFile.txt");
        DllWrapper.OpenFile(path, out var file);

        var buffer = new Byte[1000];

        var r = DllWrapper.ReadFile(file, 1000, buffer, out var actualSize);

        ClassicAssert.AreEqual(21, actualSize);
        ClassicAssert.AreEqual("This is my text file.", Encoding.Default.GetString(buffer, 0, (int)actualSize));

        file.Dispose();
    }

    [Test]
    public void ReadFile_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.ReadFile(new UnityFileHandle(), 10, new byte[10], out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void OpenFile_ArchiveFileSystem_ReturnSuccess()
    {
        var r = DllWrapper.OpenFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.IsFalse(file.IsInvalid);

        file.Dispose();
    }

    [Test]
    public void CloseFile_ArchiveFileSystem_ReturnSuccess()
    {
        DllWrapper.OpenFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        Assert.DoesNotThrow(() => file.Dispose());
    }

    [Test]
    public void ReadFile_ArchiveFileSystem_ReadExpectedData()
    {
        DllWrapper.OpenFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        var buffer = new Byte[100];
        var r = DllWrapper.ReadFile(file, 100, buffer, out var actualSize);

        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual(100, actualSize);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-Data"), buffer);

        file.Dispose();
    }
}

public class DllSerializedFileTests : AssetBundleTestFixture
{
    private UnityArchiveHandle m_Archive;

    public DllSerializedFileTests(Context context) : base(context)
    {
    }
        
    [OneTimeSetUp]
    public void Setup()
    {
        DllWrapper.Init();
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        DllWrapper.MountArchive(path, "archive:/", out m_Archive);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        m_Archive.Dispose();
        DllWrapper.Cleanup();
    }

    [Test]
    public void OpenSerializedFile_InvalidPath_ReturnError()
    {
        var r = DllWrapper.OpenSerializedFile("bad/path", out _);
        ClassicAssert.AreEqual(ReturnCode.FileNotFound, r);
    }

    [Test]
    [Ignore("This test crashes, this condition is not handled properly in Unity")]
    public void OpenSerializedFile_NotSerializedFile_ReturnError()
    {
        var r = DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994.resS", out _);

        // It would be better to test for an actual error code instead of !success but it's not possible because of the way
        // the SerializedFile is implemented! When opening a SerializedFile, the header is read and if the version is higher than some value,
        // the kSerializedFileLoadError_HigherSerializedFileVersion error is returned. If it's not, then it will be another error. There's no
        // signature to prevent this kind of issue.
        ClassicAssert.AreNotEqual(ReturnCode.Success, r);
    }

    [Test]
    public void OpenSerializedFile_ValidSerializedFile_ReturnSuccess()
    {
        var r = DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        file.Dispose();
    }

    [Test]
    public void CloseSerializedFile_ValidSerializedFile_ReturnSuccess()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);
        Assert.DoesNotThrow(() => file.Dispose());
    }

    [Test]
    public void CloseSerializedFile_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.CloseSerializedFile(IntPtr.Zero);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetExternalReferenceCount_ValidSerializedFile_ReturnExpectedExternalReferenceCount()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        var r = DllWrapper.GetExternalReferenceCount(file, out var count);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-ExtRefCount"), count);

        file.Dispose();
    }

    [Test]
    public void GetExternalReferenceCount_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetExternalReferenceCount(new SerializedFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetExternalReference_StringTooSmall_ReturnError()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        var path = new StringBuilder(10);
        var guid = new StringBuilder(32);
        var r = DllWrapper.GetExternalReference(file, 0, path, 10, guid, out var refType);
        ClassicAssert.AreEqual(ReturnCode.DestinationBufferTooSmall, r);

        file.Dispose();
    }

    [Test]
    public void GetExternalReference_ValidSerializedFile_ReturnExpectedExternalReferences()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        DllWrapper.GetExternalReferenceCount(file, out var count);
        var path = new StringBuilder(256);
        var guid = new StringBuilder(32);
            
        for (int i = 0; i < count; ++i)
        {
            var r = DllWrapper.GetExternalReference(file, i, path, 256, guid, out var refType);
                
            ClassicAssert.AreEqual(ReturnCode.Success, r);
            ClassicAssert.AreEqual(Context.ExpectedData.Get($"CAB-5d40f7cad7c871cf2ad2af19ac542994-ExtRef{i}-Guid"), guid.ToString());
            ClassicAssert.AreEqual(Context.ExpectedData.Get($"CAB-5d40f7cad7c871cf2ad2af19ac542994-ExtRef{i}-Path"), path.ToString());
            ClassicAssert.AreEqual(Context.ExpectedData.Get($"CAB-5d40f7cad7c871cf2ad2af19ac542994-ExtRef{i}-Type"), (long)refType);
        }

        file.Dispose();
    }

    [Test]
    public void GetExternalReference_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetExternalReferenceCount(new SerializedFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetObjectCount_ValidSerializedFile_ReturnExpectedObjectCount()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);

        var r = DllWrapper.GetObjectCount(file, out var count);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-ObjCount"), count);

        file.Dispose();
    }

    [Test]
    public void GetObjectCount_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetObjectCount(new SerializedFileHandle(), out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetObjectInfo_ValidSerializedFile_ReturnExpectedObjectInfo()
    {
        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out var file);
        DllWrapper.GetObjectCount(file, out var count);

        var objectInfo = new ObjectInfo[count];
        var r = DllWrapper.GetObjectInfo(file, objectInfo, count);
        ClassicAssert.AreEqual(ReturnCode.Success, r);

        // Just make sure that first and last ObjectInfo struct are filled.

        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-FirstObj-Id"), objectInfo[0].Id);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-FirstObj-Offset"), objectInfo[0].Offset);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-FirstObj-Size"), objectInfo[0].Size);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-FirstObj-TypeId"), objectInfo[0].TypeId);

        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-LastObj-Id"), objectInfo[count-1].Id);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-LastObj-Offset"), objectInfo[count-1].Offset);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-LastObj-Size"), objectInfo[count-1].Size);
        ClassicAssert.AreEqual(Context.ExpectedData.Get("CAB-5d40f7cad7c871cf2ad2af19ac542994-LastObj-TypeId"), objectInfo[count-1].TypeId);

        file.Dispose();
    }

    [Test]
    public void GetObjectInfo_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetObjectInfo(new SerializedFileHandle(), new ObjectInfo[0], 0);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }
}

public class DllTypeTreeTests : AssetBundleTestFixture
{
    private UnityArchiveHandle      m_Archive;
    private SerializedFileHandle    m_SerializedFile;
    private ObjectInfo[]            m_Objects;

    public DllTypeTreeTests(Context context) : base(context)
    {
    }
        
    [OneTimeSetUp]
    public void Setup()
    {
        DllWrapper.Init();
        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        DllWrapper.MountArchive(path, "archive:/", out m_Archive);

        DllWrapper.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", out m_SerializedFile);
        DllWrapper.GetObjectCount(m_SerializedFile, out var count);
        m_Objects = new ObjectInfo[count];
        DllWrapper.GetObjectInfo(m_SerializedFile, m_Objects, m_Objects.Length);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        m_SerializedFile.Dispose();
        m_Archive.Dispose();
        DllWrapper.Cleanup();
    }

    [Test]
    public void GetTypeTree_InvalidObjectId_ReturnError()
    {
        var r = DllWrapper.GetTypeTree(m_SerializedFile, 0, out var typeTree);
        ClassicAssert.AreEqual(ReturnCode.InvalidObjectId, r);
    }

    [Test]
    public void GetTypeTree_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetTypeTree(new SerializedFileHandle(), 0, out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetTypeTree_ValidSerializedFile_ReturnSuccess()
    {
        var r = DllWrapper.GetTypeTree(m_SerializedFile, m_Objects[0].Id, out var typeTree);
        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.IsFalse(typeTree.IsInvalid);
    }

    [Test]
    public void GetTypeTreeNodeInfo_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetTypeTreeNodeInfo(new TypeTreeHandle(), 0, new StringBuilder(), 0, new StringBuilder(), 0, out _, out _, out _, out _, out _, out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetTypeTreeNodeInfo_TypeStringTooSmall_ReturnError()
    {
        DllWrapper.GetTypeTree(m_SerializedFile, m_Objects[0].Id, out var typeTree);
        var r = DllWrapper.GetTypeTreeNodeInfo(typeTree, 0, new StringBuilder(1), 1, new StringBuilder(256), 256, out _, out _, out _, out _, out _, out _);
        ClassicAssert.AreEqual(ReturnCode.DestinationBufferTooSmall, r);
    }

    [Test]
    public void GetTypeTreeNodeInfo_NameStringTooSmall_ReturnError()
    {
        DllWrapper.GetTypeTree(m_SerializedFile, m_Objects[0].Id, out var typeTree);
        var r = DllWrapper.GetTypeTreeNodeInfo(typeTree, 0, new StringBuilder(256), 256, new StringBuilder(1), 1, out _, out _, out _, out _, out _, out _);
        ClassicAssert.AreEqual(ReturnCode.DestinationBufferTooSmall, r);
    }

    [Test]
    public void GetTypeTreeNodeInfo_ValidData_ReturnExpectedValues()
    {
        foreach (var obj in m_Objects)
        {
            DllWrapper.GetTypeTree(m_SerializedFile, obj.Id, out var typeTree);
            var type = new StringBuilder(256);
            var name = new StringBuilder(256);
            var r = DllWrapper.GetTypeTreeNodeInfo(typeTree, 0, type, type.Capacity, name, name.Capacity, out var offset, out var size, out var flags, out var metaFlags, out var firstChildNode, out var nextNode);
            ClassicAssert.AreEqual(ReturnCode.Success, r);
            ClassicAssert.AreNotEqual("", type.ToString());
            ClassicAssert.AreEqual("Base", name.ToString());
            ClassicAssert.AreEqual(-1, offset);
            ClassicAssert.AreNotEqual(0, size);
            ClassicAssert.AreEqual(TypeTreeFlags.None, flags);
            ClassicAssert.AreEqual(1, firstChildNode);
            ClassicAssert.AreEqual(0, nextNode);
        }
    }

    [Test]
    public void GetTypeTreeNodeInfo_IterateAll_ReturnExpectedValues()
    {
        int ProcessNode(TypeTreeHandle typeTree, int nodeIndex)
        {
            var type = new StringBuilder(256);
            var name = new StringBuilder(256);
            var r = DllWrapper.GetTypeTreeNodeInfo(typeTree, nodeIndex, type, type.Capacity, name, name.Capacity, out var offset, out var size, out var flags, out var metaFlags, out var nextChild, out var nextNode);

            ClassicAssert.AreEqual(ReturnCode.Success, r);
            ClassicAssert.AreNotEqual("", type.ToString());
            ClassicAssert.AreNotEqual("", name.ToString());
            ClassicAssert.GreaterOrEqual(offset, -1);
            ClassicAssert.GreaterOrEqual(size, -1);

            while (nextChild != 0)
            {
                nextChild = ProcessNode(typeTree, nextChild);
            }

            return nextNode;
        }

        foreach (var obj in m_Objects)
        {
            DllWrapper.GetTypeTree(m_SerializedFile, obj.Id, out var typeTree);
            var type = new StringBuilder(256);
            var name = new StringBuilder(256);
            var r = DllWrapper.GetTypeTreeNodeInfo(typeTree, 0, type, type.Capacity, name, name.Capacity, out var offset, out var size, out var flags, out var metaFlags, out var firstChildNode, out var nextNode);

            ProcessNode(typeTree, firstChildNode);
        }
    }

    [Test]
    public void GetRefTypeTypeTree_InvalidHandle_ReturnError()
    {
        var r = DllWrapper.GetRefTypeTypeTree(new SerializedFileHandle(), "", "", "", out _);
        ClassicAssert.AreEqual(ReturnCode.InvalidArgument, r);
    }

    [Test]
    public void GetRefTypeTypeTree_InvalidFQN_ReturnError()
    {
        var r = DllWrapper.GetRefTypeTypeTree(m_SerializedFile, "this", "is", "wrong", out _);
        ClassicAssert.AreEqual(ReturnCode.TypeNotFound, r);
    }

    [Test]
    public void GetRefTypeTree_ValidSerializedFile_ReturnSuccess()
    {
        var r = DllWrapper.GetRefTypeTypeTree(m_SerializedFile, "SerializeReferencePolymorphismExample/Apple", "", "Assembly-CSharp", out var typeTree);

        ClassicAssert.AreEqual(ReturnCode.Success, r);
    }

    [Test]
    public void GetTypeTreeNodeInfo_RefTypeTypeTree_ReturnExpectedValues()
    {
        var r = DllWrapper.GetRefTypeTypeTree(m_SerializedFile, "SerializeReferencePolymorphismExample/Apple", "", "Assembly-CSharp", out var typeTree);

        ClassicAssert.AreEqual(ReturnCode.Success, r);

        var type = new StringBuilder(256);
        var name = new StringBuilder(256);
        r = DllWrapper.GetTypeTreeNodeInfo(typeTree, 0, type, type.Capacity, name, name.Capacity, out var offset, out var size, out var flags, out var metaFlags, out var firstChildNode, out var nextNode);

        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual(1, firstChildNode);
        ClassicAssert.AreEqual("Apple", type.ToString());
        ClassicAssert.AreEqual("Base", name.ToString());

        r = DllWrapper.GetTypeTreeNodeInfo(typeTree, firstChildNode, type, type.Capacity, name, name.Capacity, out offset, out size, out flags, out metaFlags, out firstChildNode, out nextNode);

        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual("int", type.ToString());
        ClassicAssert.AreEqual("m_Data", name.ToString());
        ClassicAssert.AreEqual(4, size);

        r = DllWrapper.GetTypeTreeNodeInfo(typeTree, nextNode, type, type.Capacity, name, name.Capacity, out offset, out size, out flags, out metaFlags, out firstChildNode, out nextNode);

        ClassicAssert.AreEqual(ReturnCode.Success, r);
        ClassicAssert.AreEqual("string", type.ToString());
        ClassicAssert.AreEqual("m_Description", name.ToString());
    }
}
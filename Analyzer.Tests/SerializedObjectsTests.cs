using NUnit.Framework;
using NUnit.Framework.Legacy;
using UnityDataTools.FileSystem;
using UnityDataTools.FileSystem.TypeTreeReaders;
using UnityDataTools.Analyzer.SerializedObjects;
using UnityDataTools.TestCommon;
using UnityDataTools.UnityDataTool.Tests;

namespace UnityDataTools.Analyzer.Tests;

#pragma warning disable NUnit2005, NUnit2006

public class SerializedObjectsTests : AssetBundleTestFixture
{
    private UnityArchive m_Archive;
    private SerializedFile m_SerializedFile;
    private UnityFileReader m_FileReader;

    public SerializedObjectsTests(Context context) : base(context)
    {
    }
    
    protected override void OnLoadExpectedData(Context context)
    {
        // Uncomment to regenerate expected data.
        //ExpectedDataGenerator.Generate(context);
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        UnityFileSystem.Init();

        var path = Path.Combine(Context.UnityDataFolder, "assetbundle");
        m_Archive = UnityFileSystem.MountArchive(path, "archive:/");
        m_SerializedFile = UnityFileSystem.OpenSerializedFile("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994");
        m_FileReader = new UnityFileReader("archive:/CAB-5d40f7cad7c871cf2ad2af19ac542994", 1024*1024);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        m_FileReader.Dispose();
        m_SerializedFile.Dispose();
        m_Archive.Dispose();

        UnityFileSystem.Cleanup();
    }
    
    T ReadObject<T>(long id, Func<RandomAccessReader, T> creator)
    {
        var objectInfo = m_SerializedFile.Objects.First(x => x.Id == id);
        var node = m_SerializedFile.GetTypeTreeRoot(objectInfo.Id);
        var reader = new RandomAccessReader(m_SerializedFile, node, m_FileReader, objectInfo.Offset);
        return creator(reader);
    }

    [TestCase("Texture1", -9023202112035587373)]
    [TestCase("Texture2", 404836592933730457)]
    public void TestTexture2d(string name, long id)
    {
        var texture = ReadObject(id, Texture2D.Read);
        var expectedTexture = (Texture2D)Context.ExpectedData.Get(name);
        
        ClassicAssert.AreEqual(expectedTexture.Name, texture.Name);
        ClassicAssert.AreEqual(expectedTexture.StreamDataSize, texture.StreamDataSize);
        ClassicAssert.AreEqual(expectedTexture.Width, texture.Width);
        ClassicAssert.AreEqual(expectedTexture.Height, texture.Height);
        ClassicAssert.AreEqual(expectedTexture.Format, texture.Format);
        ClassicAssert.AreEqual(expectedTexture.MipCount, texture.MipCount);
        ClassicAssert.AreEqual(expectedTexture.RwEnabled, texture.RwEnabled);
    }
    
    [Test]
    public void TestAnimationClip()
    {
        var clip = ReadObject(2152370074763270995, AnimationClip.Read);
        var expectedClip = (AnimationClip)Context.ExpectedData.Get("AnimationClip");
        
        ClassicAssert.AreEqual(expectedClip.Name, clip.Name);
        ClassicAssert.AreEqual(expectedClip.Events, clip.Events);
        ClassicAssert.AreEqual(expectedClip.Legacy, clip.Legacy);
    }
    
    [Test]
    public void TestAudioClip()
    {
        var clip = ReadObject(-8074603400156879931, AudioClip.Read);
        var expectedClip = (AudioClip)Context.ExpectedData.Get("AudioClip");
        
        ClassicAssert.AreEqual(expectedClip.Name, clip.Name);
        ClassicAssert.AreEqual(expectedClip.Channels, clip.Channels);
        ClassicAssert.AreEqual(expectedClip.Format, clip.Format);
        ClassicAssert.AreEqual(expectedClip.Frequency, clip.Frequency);
        ClassicAssert.AreEqual(expectedClip.LoadType, clip.LoadType);
        ClassicAssert.AreEqual(expectedClip.BitsPerSample, clip.BitsPerSample);
        ClassicAssert.AreEqual(expectedClip.StreamDataSize, clip.StreamDataSize);
    }
    
    [Test]
    public void TestAssetBundle()
    {
        var bundle = ReadObject(1, AssetBundle.Read);
        var expectedBundle = (AssetBundle)Context.ExpectedData.Get("AssetBundle");
        
        ClassicAssert.AreEqual(expectedBundle.Name, bundle.Name);
        ClassicAssert.AreEqual(expectedBundle.Assets.Count, bundle.Assets.Count);

        for (int i = 0; i < bundle.Assets.Count; ++i)
        {
            var asset = bundle.Assets[i];
            var expectedAsset = expectedBundle.Assets[i];
            
            ClassicAssert.AreEqual(expectedAsset.Name, asset.Name);
            ClassicAssert.AreEqual(expectedAsset.PPtr.FileId, asset.PPtr.FileId);
            ClassicAssert.AreEqual(expectedAsset.PPtr.PathId, asset.PPtr.PathId);
        }
    }
    
    [Test]
    public void TestMesh()
    {
        var mesh = ReadObject(4693305862354978555, Mesh.Read);
        var expectedMesh = (Mesh)Context.ExpectedData.Get("Mesh");
        
        ClassicAssert.AreEqual(expectedMesh.Name, mesh.Name);
        ClassicAssert.AreEqual(expectedMesh.Bones, mesh.Bones);
        ClassicAssert.AreEqual(expectedMesh.Compression, mesh.Compression);
        ClassicAssert.AreEqual(expectedMesh.Indices, mesh.Indices);
        ClassicAssert.AreEqual(expectedMesh.Vertices, mesh.Vertices);
        ClassicAssert.AreEqual(expectedMesh.BlendShapes, mesh.BlendShapes);
        ClassicAssert.AreEqual(expectedMesh.RwEnabled, mesh.RwEnabled);
        ClassicAssert.AreEqual(expectedMesh.StreamDataSize, mesh.StreamDataSize);
        
        ClassicAssert.AreEqual(expectedMesh.Channels.Count, mesh.Channels.Count);

        for (int i = 0; i < mesh.Channels.Count; ++i)
        {
            var channel = mesh.Channels[i];
            var expectedChannel = expectedMesh.Channels[i];
            
            ClassicAssert.AreEqual(expectedChannel.Dimension, channel.Dimension);
            ClassicAssert.AreEqual(expectedChannel.Type, channel.Type);
            ClassicAssert.AreEqual(expectedChannel.Usage, channel.Usage);
        }
    }

    [Test]
    public void TestShaderReader()
    {
        var shader = ReadObject(-4850512016903265157, Shader.Read);
        var expectedShader = (Shader)Context.ExpectedData.Get("Shader");
        
        ClassicAssert.AreEqual(expectedShader.Name, shader.Name);
        ClassicAssert.AreEqual(expectedShader.DecompressedSize, shader.DecompressedSize);
        CollectionAssert.AreEquivalent(expectedShader.Keywords, shader.Keywords);
        ClassicAssert.AreEqual(expectedShader.SubShaders.Count, shader.SubShaders.Count);

        for (int i = 0; i < shader.SubShaders.Count; ++i)
        {
            var subShader = shader.SubShaders[i];
            var expectedSubShader = shader.SubShaders[i];
            
            ClassicAssert.AreEqual(expectedSubShader.Passes.Count, subShader.Passes.Count);

            for (int j = 0; j < subShader.Passes.Count; ++j)
            {
                var pass = subShader.Passes[i];
                var expectedPass = expectedSubShader.Passes[i];
                
                ClassicAssert.AreEqual(expectedPass.Name, pass.Name);
                ClassicAssert.AreEqual(expectedPass.Programs.Count, pass.Programs.Count);
                CollectionAssert.AreEquivalent(expectedPass.Programs.Keys, pass.Programs.Keys);

                foreach (var programsPerType in pass.Programs)
                {
                    var programs = programsPerType.Value;
                    var expectedPrograms = expectedPass.Programs[programsPerType.Key];

                    ClassicAssert.AreEqual(expectedPrograms.Count, programs.Count);

                    for (int k = 0; k < programs.Count; ++k)
                    {
                        var program = programs[k];
                        var expectedProgram = expectedPrograms[k];
                        
                        ClassicAssert.AreEqual(expectedProgram.Api, program.Api);
                        ClassicAssert.AreEqual(expectedProgram.BlobIndex, program.BlobIndex);
                        ClassicAssert.AreEqual(expectedProgram.HwTier, program.HwTier);
                        CollectionAssert.AreEquivalent(expectedProgram.Keywords, program.Keywords);
                    }
                }
            }
        }
    }
}

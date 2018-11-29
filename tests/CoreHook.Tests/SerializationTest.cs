using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using Xunit;
using CoreHook.CoreLoad.Data;
using System.Runtime.InteropServices;

namespace CoreHook.Tests
{
    public class SerializationTest
    {
        [Fact]
        void ShouldThrowErrorWhenSerializingNullManagedRemoteInfoClass()
        {
            ManagedRemoteInfo remoteInfo = null;

            Assert.Throws<ArgumentNullException>(
                () => new BinaryFormatter().Serialize(new MemoryStream(), remoteInfo));
        }

        [Fact]
        void ShouldThrowErrorWhenSerializingNullStreamClass()
        {
            MemoryStream memoryStream = null;
            ManagedRemoteInfo remoteInfo = new ManagedRemoteInfo();

            Assert.Throws<ArgumentNullException>(
                () => new BinaryFormatter().Serialize(memoryStream, remoteInfo));
        }

        [Fact]
        void ShouldSerializeAndDeserializeManagedRemoteInfoObject()
        {
            var memoryStream = new MemoryStream();
            var remoteInfo = new ManagedRemoteInfo();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, remoteInfo);

            memoryStream.Position = 0;
            object deserializedRemoteInfo = binaryFormatter.Deserialize(memoryStream);

            Assert.NotNull(deserializedRemoteInfo);
        }

        [Fact]
        void ShouldSerializeAndDeserializeManagedRemoteInfoClass()
        {
            var memoryStream = new MemoryStream();
            var remoteInfo = new ManagedRemoteInfo();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, remoteInfo);

            memoryStream.Position = 0;
            object deserializedRemoteInfo = binaryFormatter.Deserialize(memoryStream);

            Assert.NotNull(deserializedRemoteInfo);
            Assert.IsType(typeof(ManagedRemoteInfo), deserializedRemoteInfo);
        }

        [Fact]
        void ShouldSerializeAndDeserializeManagedRemoteInfoChannelName()
        {
            const string channelName = "ChannelName";
            var memoryStream = new MemoryStream();
            var remoteInfo = new ManagedRemoteInfo
            {
                ChannelName = channelName
            };
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, remoteInfo);

            memoryStream.Position = 0;
            object deserializedRemoteInfo = binaryFormatter.Deserialize(memoryStream);

            Assert.NotNull(deserializedRemoteInfo);
            Assert.IsType(typeof(ManagedRemoteInfo), deserializedRemoteInfo);
            Assert.Equal(channelName, ((ManagedRemoteInfo)deserializedRemoteInfo).ChannelName);
        }
    }
}

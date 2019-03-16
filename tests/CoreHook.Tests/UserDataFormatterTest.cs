using System;
using System.IO;
using CoreHook.CoreLoad.Data;
using Xunit;

namespace CoreHook.Tests
{
    public class UserDataFormatterTest
    {
        [Serializable]
        internal class  UserDataFormatterTestClass
        {
            internal int IntegerMember;
        }

        [Fact]
        public void ShouldThrowNullExceptionWhenSerializingNullObject()
        {
            IUserDataFormatter formatter = CreateFormatter();
            Stream serializationStream = new MemoryStream();
            object objectToSerialize = null;

            Assert.Throws<ArgumentNullException>(() => formatter.Serialize(serializationStream, objectToSerialize));
        }

        [Fact]
        public void ShouldThrowNullExceptionWhenSerializingWithNullStream()
        {
            IUserDataFormatter formatter = CreateFormatter();
            Stream serializationStream = null;
            var objectToSerialize = new byte[4];

            Assert.Throws<ArgumentNullException>(() => formatter.Serialize(serializationStream, objectToSerialize));
        }

        [Fact]
        public void ShouldThrowNullExceptionWhenSerializingWithNullStreamAndNullObject()
        {
            IUserDataFormatter formatter = CreateFormatter();
            Stream serializationStream = null;
            object objectToSerialize = null;

            Assert.Throws<ArgumentNullException>(() => formatter.Serialize(serializationStream, objectToSerialize));
        }

        [Fact]
        public void ShouldSerializeClassToStream()
        {
            IUserDataFormatter formatter = CreateFormatter();

            using (Stream serializationStream = new MemoryStream())
            {
                var objectToSerialize = new UserDataFormatterTestClass {IntegerMember = 1};
                formatter.Serialize(serializationStream, objectToSerialize);

                Assert.NotEqual(0, serializationStream.Length);
            }
        }

        [Fact]
        public void ShouldSerializeAndDeserializeClassWithStream()
        {
            IUserDataFormatter formatter = CreateFormatter();
            const int integerMemberValue = 1;

            using (Stream serializationStream = new MemoryStream())
            {
                var objectToSerialize = new UserDataFormatterTestClass { IntegerMember = integerMemberValue };
                formatter.Serialize(serializationStream, objectToSerialize);

                Assert.NotEqual(0, serializationStream.Length);

                serializationStream.Position = 0;
                var deserializedObject = formatter.Deserialize<UserDataFormatterTestClass>(serializationStream);

                Assert.NotNull(deserializedObject);
                Assert.Equal(integerMemberValue, deserializedObject.IntegerMember);
            }
        }

        private static IUserDataFormatter CreateFormatter() => new UserDataBinaryFormatter();
    }
}

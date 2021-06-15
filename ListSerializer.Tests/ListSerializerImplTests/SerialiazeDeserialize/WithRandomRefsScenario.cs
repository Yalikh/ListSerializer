using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ListSerializer.Tests.ListSerializerImplTests.SerialiazeDeserialize
{
    public class WithRandomRefsScenario
    {
        ListNode Input { get; set; }

        void Arrange()
        {
            var head = new ListNode
            {
                Data = "head",
            };
            var tail = new ListNode
            {
                Data = "tail",
                Previous = head,
                Random = head
            };
            head.Next = tail;

            Input = head;
        }

        async Task<ListNode> Act()
        {
            using var stream = new MemoryStream();

            var service = new ListSerializerImpl();
            await service.Serialize(Input, stream);
            stream.Seek(0, SeekOrigin.Begin);

            return await service.Deserialize(stream);
        }

        [Fact]
        public async Task HeadRandomRefIsNull()
        {
            Arrange();

            var actual = await Act();

            Assert.Null(actual.Random);
        }

        [Fact]
        public async Task TailRandomRefIsSpecified()
        {
            Arrange();

            var actual = await Act();

            Assert.Equal(actual, actual.Next.Random);
        }
    }
}

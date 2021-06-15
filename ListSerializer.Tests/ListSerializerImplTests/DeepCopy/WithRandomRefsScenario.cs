using System.Threading.Tasks;
using Xunit;

namespace ListSerializer.Tests.ListSerializerImplTests.DeepCopy
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

        Task<ListNode> Act()
        {
            return new ListSerializerImpl().DeepCopy(Input);
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

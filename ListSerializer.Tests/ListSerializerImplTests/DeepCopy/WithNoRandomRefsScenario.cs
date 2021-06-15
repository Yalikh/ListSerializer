using System.Threading.Tasks;
using Xunit;

namespace ListSerializer.Tests.ListSerializerImplTests.DeepCopy
{
    public class WithNoRandomRefsScenario
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
                Previous = head
            };
            head.Next = tail;

            Input = head;
        }

        Task<ListNode> Act()
        {
            return new ListSerializerImpl().DeepCopy(Input);
        }

        [Fact]
        public async Task HeadIsNotNull()
        {
            Arrange();

            var actual = await Act();

            Assert.NotNull(actual);
        }

        [Fact]
        public async Task HeadHasCorrectRef()
        {
            Arrange();

            var actual = await Act();

            Assert.Null(actual.Previous);
        }

        [Fact]
        public async Task TailIsNotNull()
        {
            Arrange();

            var actual = await Act();

            Assert.NotNull(actual.Next);
        }

        [Fact]
        public async Task TailHasCorrectRef()
        {
            Arrange();

            var actual = await Act();

            Assert.Null(actual.Next.Next);
        }

        [Fact]
        public async Task HeadHasCorrectData()
        {
            Arrange();

            var actual = await Act();

            Assert.Equal(Input.Data, actual.Data);
        }

        [Fact]
        public async Task TailHasCorrectData()
        {
            Arrange();

            var actual = await Act();

            Assert.Equal(Input.Next.Data, actual.Next.Data);
        }

        [Fact]
        public async Task RandomRefsAreNull()
        {
            Arrange();

            var actual = await Act();

            Assert.Null(actual.Random);
            Assert.Null(actual.Next.Random);
        }
    }
}

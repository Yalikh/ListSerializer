## Test

Consider the following data structure and interface:
``` 
    /// <summary>
    /// Node of the list with random links
    /// </summary>
    class ListNode
    {
        /// <summary>
        /// Ref to the previous node in the list, null for head
        /// </summary>
        public ListNode Previous;

        /// <summary>
        /// Ref to the next node in the list, null for tail
        /// </summary>
        public ListNode Next;

        /// <summary>
        /// Ref to the random node in the list, could be null
        /// </summary>
        public ListNode Random;

        /// <summary>
        /// Payload
        /// </summary>
        public string Data;
    }

    /// <summary>
    /// Serializer interface for list based on the ListNode
    /// </summary>
    interface IListSerializer
    {

        /// <summary>
        /// Serializes all nodes in the list, including topology of the Random links, into stream
        /// </summary>
        Task Serialize(ListNode head, Stream s)

        /// <summary>
        /// Deserializes the list from the stream, returns the head node of the list
        /// </summary>
		/// <exception cref="System.ArgumentException">Thrown when a stream has invalid data</exception>
        Task<ListNode> Deserialize(Stream s)

        /// <summary>
        /// Makes a deep copy of the list, returns the head node of the list 
        /// </summary>
        Task<ListNode> DeepCopy(ListNode head)

    }
```

Please provide the following:

1. Develop class implementing IListSerializer interface for time critical part of the back-end application. You can use any serialization format but you could not utilize 3d party libraries that will serialize the full list for you. I.e. it's allowed to utilize 3d party libraries for serializing 1 node in particular format.
Note:
- provided data structures could not be changed;
- solution is allowed to be not thread safe;
- it's guaranteed that list provided as an argument to Serialize and DeepCopy function is consistent and doesn't contain any cycles;
- automated testing of your solution will be performed, the resulting rate for the solution will be given based on (in order of priority):
  - tests on correctness of the solution 
  - performance tests 
  - tests on memory consumption

2. Provide your own test cases for the implementation.
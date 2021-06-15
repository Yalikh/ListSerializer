using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Buffers;

namespace ListSerializer
{
    public class ListSerializerImpl : IListSerializer
    {
        public Task<ListNode> DeepCopy(ListNode head)
        {
            return Task.Run(() =>
            {
                // Key is copied node has not null Random prop
                // Value is original node from the Random prop
                var randomRefs = new Dictionary<ListNode, ListNode>();

                // Key is original node
                // Value is copied node
                var copiesMap = new Dictionary<ListNode, ListNode>();

                var curNode = head;
                var copiedNode = CopyNode(head, null, copiesMap);
                var copiedHead = copiedNode;

                while (curNode != null)
                {
                    ListNode copiedNext = null;
                    if (curNode.Next != null)
                    {
                        copiedNext = CopyNode(curNode.Next, copiedNode, copiesMap);
                        copiedNode.Next = copiedNext;
                    }

                    if (curNode.Random != null)
                        randomRefs.Add(copiedNode, curNode.Random);

                    curNode = curNode.Next;
                    copiedNode = copiedNext;
                }

                foreach (var randomRef in randomRefs)
                {
                    randomRef.Key.Random = copiesMap[randomRef.Value];
                }

                return copiedHead;
            });
        }

        public Task<ListNode> Deserialize(Stream s)
        {
            return Task.Run(() =>
            {
                var buffer = new byte[s.Length];
                s.Read(buffer);

                var reader = new Utf8JsonReader(buffer, false, default);

                ListNode curNode = null;
                var nodeMap = new Dictionary<Guid, ListNode>();
                var nextRefMap = new Dictionary<ListNode, Guid>();
                var randomRefMap = new Dictionary<ListNode, Guid>();
                string curProp = null;
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                            curNode = new ListNode();
                            break;
                        case JsonTokenType.EndObject:
                            curNode = null;
                            curProp = null;
                            break;
                        case JsonTokenType.PropertyName:
                            curProp = reader.GetString();
                            break;
                        case JsonTokenType.String:
                            switch (curProp)
                            {
                                case "id":
                                    nodeMap.Add(reader.GetGuid(), curNode);
                                    break;
                                case "data":
                                    var data = 
                                    curNode.Data = reader.GetString();
                                    break;
                                case "previous":
                                    curNode.Previous = nodeMap[reader.GetGuid()];
                                    break;
                                case "next":
                                    if (!nextRefMap.ContainsKey(curNode))
                                        nextRefMap.Add(curNode, reader.GetGuid());
                                    break;
                                case "random":
                                    if (!randomRefMap.ContainsKey(curNode))
                                        randomRefMap.Add(curNode, reader.GetGuid());
                                    break;
                            }
                            break;
                    }
                }

                foreach(var nextRef in nextRefMap)
                {
                    if (!nodeMap.TryGetValue(nextRef.Value, out var nextNode))
                        throw new Exception($"Unknown next ref\"{nextRef.Value}\"");
                        
                    nextRef.Key.Next = nextNode;
                }

                foreach (var randomRef in randomRefMap)
                {
                    if (!nodeMap.TryGetValue(randomRef.Value, out var randomNode))
                        throw new Exception($"Unknown random ref\"{randomRef.Value}\"");

                    randomRef.Key.Random = randomNode;
                }

                return nodeMap.Values.Single(x => x.Previous == null);
            });
        }

        public Task Serialize(ListNode head, Stream s)
        {
            return Task.Run(() =>
            {
                using var writer = new Utf8JsonWriter(s);
                writer.WriteStartArray();

                var nodeMap = new Dictionary<ListNode, Guid>();

                var curNode = head;
                while (curNode != null)
                {
                    SerializeNode(curNode, writer, nodeMap);

                    curNode = curNode.Next;
                }

                writer.WriteEndArray();
                writer.Flush();
            });
        }

        ListNode CopyNode(ListNode source, ListNode prevNodeCopy, Dictionary<ListNode, ListNode> copiesMap)
        {
            var result = new ListNode
            {
                Data = source.Data,
                Previous = prevNodeCopy,
            };

            copiesMap.Add(source, result);

            return result;
        }

        void SerializeNode(ListNode node, Utf8JsonWriter writer, Dictionary<ListNode, Guid> nodeMap)
        {
            writer.WriteStartObject();
            writer.WriteString("id", GetOrAddId(nodeMap, node));
            writer.WriteString("data", node.Data);

            if (node.Previous != null)
                writer.WriteString("previous", GetOrAddId(nodeMap, node.Previous));

            if (node.Next != null)
                writer.WriteString("next", GetOrAddId(nodeMap, node.Next));

            if (node.Random != null)
                writer.WriteString("random", GetOrAddId(nodeMap, node.Random));

            writer.WriteEndObject();
        }

        Guid GetOrAddId(Dictionary<ListNode, Guid> nodeMap, ListNode key)
        {
            if (!nodeMap.TryGetValue(key, out var id))
            {
                id = Guid.NewGuid();
                nodeMap.Add(key, id);
            }

            return id;
        }

        void GetMoreBytesFromStream(Stream stream, ref byte[] buffer, ref Utf8JsonReader reader)
        {
            int bytesRead;
            if (reader.BytesConsumed < buffer.Length)
            {
                ReadOnlySpan<byte> leftover = buffer.AsSpan((int)reader.BytesConsumed);

                if (leftover.Length == buffer.Length)
                    Array.Resize(ref buffer, buffer.Length * 2);

                leftover.CopyTo(buffer);
                bytesRead = stream.Read(buffer.AsSpan(leftover.Length));
            }
            else
            {
                bytesRead = stream.Read(buffer);
            }

            reader = new Utf8JsonReader(buffer, isFinalBlock: bytesRead == 0, reader.CurrentState);
        }
    }
}

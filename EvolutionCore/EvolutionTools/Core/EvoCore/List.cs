using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionTools
{
    public class SortedValueList<T> : List<T>    
    {
        public List<double> Values = new List<double>();
        public List<object> Tags = new List<object>();
        public string ValueOperand = "";

        public SortedValueList(string valueOperand) : base()
        {
            this.ValueOperand = valueOperand;
        }
        public SortedValueList(string valueOperand, T[] objs, double[] vals, object[] tags)
            : this(valueOperand, objs, vals, tags, false)
        {
        }
        public SortedValueList(string valueOperand, T[] objs, double[] vals, object[] tags, bool ignoreNull)
            : this(valueOperand)
        {
            if (tags == null)
                tags = new object[objs.Length];

            for (int i = 0; i < objs.Length; i++)
            {
                if (ignoreNull && objs[i] == null)
                    continue;

                this.Add(objs[i], vals[i], tags[i]);
            }
        }


        public void Add(T obj, double value, object tag)
        {  
            if (this.Values.Count == 0)
            {
                this.Add(obj);
                this.Values.Add(value);
                this.Tags.Add(tag);
                return;
            }

            if (!this.ValueOperand.Contains(">") && !this.ValueOperand.Contains("<"))
                throw new Exception("Invalid value Operand");

            var index = 0;
            while (index < this.Values.Count) 
            {
                if ((this.ValueOperand == ">" && value > this.Values[index]) ||
                    (this.ValueOperand == ">=" && value >= this.Values[index]) ||
                    (this.ValueOperand == "<" && value < this.Values[index]) ||
                    (this.ValueOperand == "<=" && value <= this.Values[index]))
                {
                    this.Insert(index, obj);
                    this.Values.Insert(index, value);
                    this.Tags.Insert(index, tag);
                    return;
                }

                index++;
            }

            this.Add(obj);
            this.Values.Add(value);
            this.Tags.Add(tag);
        }
        public new void Remove(T obj)
        {
            var i = this.IndexOf(obj);

            if (i == -1)
                return;

            this.RemoveAt(i);
            this.Values.RemoveAt(i);
            this.Tags.RemoveAt(i);
        }
    }
    public class MultiSortedValueList<T> : List<T>
    {
        public List<double[]> AllValues = new List<double[]>();
        public List<object> Tags = new List<object>();
        public string[] ValueOperands;

        public MultiSortedValueList(string[] valueOperands)
            : base()
        {
            this.ValueOperands = valueOperands;
        }
        public MultiSortedValueList(string valueOperand, int sortValues)
        {
            var valueOperands = new List<string>();
            for (int i = 0; i < sortValues; i++)
                valueOperands.Add(valueOperand);

            this.ValueOperands = valueOperands.ToArray<string>();
        }


        public void Add(T obj, double[] values, object tag)
        {
            if (this.AllValues.Count == 0)
            {
                this.Add(obj);
                this.AllValues.Add(values);
                this.Tags.Add(tag);
                return;
            }


            var index = 0;
            while (index < this.AllValues.Count)
            {
                var attemptInsert = false;
                var strongInsert = false;

                for (int i = 0; i < this.ValueOperands.Length; i++)
                {

                    if (!this.ValueOperands[i].Contains(">") && !this.ValueOperands[i].Contains("<"))
                        throw new Exception("Invalid value Operand");

                    if ((i == 0 || attemptInsert) && 
                        (this.ValueOperands[i] == ">" && values[i] > this.AllValues[index][i]) ||
                        (this.ValueOperands[i] == ">=" && values[i] >= this.AllValues[index][i]) ||
                        (this.ValueOperands[i] == "<" && values[i] < this.AllValues[index][i]) ||
                        (this.ValueOperands[i] == "<=" && values[i] <= this.AllValues[index][i]))
                    {
                        attemptInsert = true;

                        //If Operand contains an equal, and this is satisfied, then this level of operand is not satisfied to the point of insertion,
                        //  if further operands fail
                        if (this.ValueOperands[i].Contains("=") && values[i] == this.AllValues[index][i])
                        {
                            strongInsert = false;
                        }
                        else
                            strongInsert = true;
                   
                    }
                    else
                    {
                        //If current value operand contains the == paramemter (>=,<=)
                        //And the current value satisfies directional condition (< or >)
                        //Then override continuation of operand verification and insert directly
                        // -> Insert object into list instead of sorting based on remaining operands     

                        //If i index is greater than 0, a operand was satisfied and insertion should still take place,
                        //instead of searching along remaining indexes
                        if (!strongInsert)
                            attemptInsert = false;
                        else
                        {
                            //If obj has already satisfied a previous operand, continue with insertion
                        }

                        break;
                    }
                }
            
                if (attemptInsert)
                {
                    this.Insert(index, obj);
                    this.AllValues.Insert(index, values);
                    this.Tags.Insert(index, tag);
                    return;
                }

                index++;
            }

            this.Add(obj);
            this.AllValues.Add(values);
            this.Tags.Add(tag);
        }
        public new void Remove(T obj)
        {
            var i = this.IndexOf(obj);

            if (i == -1)
                return;

            this.RemoveAt(i);
            this.AllValues.RemoveAt(i);
            this.Tags.RemoveAt(i);
        }
    }
    public class LongList<T>
    {
        private Node<T> ZeroNode = null;
        private Node<T> EndNode = null;
        private int length = 0;

        private class Node<T>
        {
            public T Obj;
            public Node<T> Next;

            public Node(T obj)
            {
                this.Obj = obj;
            }
        }

        public LongList()
        {


        }

        public void Add(T obj)
        {
            if (this.ZeroNode == null)
                this.ZeroNode = new Node<T>(obj);

            var curNode = this.ZeroNode;

            while (curNode.Next != null)
                curNode = curNode.Next;

            curNode.Next = new Node<T>(obj);
            this.EndNode = curNode.Next;

            length++;
        }
        public void AddLast(T obj)
        {
            if (this.ZeroNode == null)
            {
                this.ZeroNode = new Node<T>(obj);
                this.EndNode = this.ZeroNode;
            }
            else
            {
                this.EndNode.Next = new Node<T>(obj);
                this.EndNode = this.EndNode.Next;
            }

            length++;
        }
        public T At(int index)
        {
            if (index >= this.length)
                throw new Exception("Invalid Index");

            var curNode = this.ZeroNode;
            var i = 0;

            while (i < index)
            {
                curNode = curNode.Next;
                i++;
            }

            return curNode.Obj;
        }
        public T Remove(int index)
        {
            if (index >= this.length)
                throw new Exception("Invalid Index");

            if (index == 0)
                this.ZeroNode = this.ZeroNode.Next;

            Node<T> prevNode = null;
            var curNode = this.ZeroNode;
            var i = 0;

            while (i < index)
            {
                prevNode = curNode;
                curNode = curNode.Next;
                i++;
            }

            if (curNode.Next == null)
                this.EndNode = prevNode;

            prevNode.Next = curNode.Next;
            return curNode.Obj;
        }

    }
}

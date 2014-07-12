﻿// Copyright (c) 2014 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.Decompiler.IL
{
	public abstract class BinaryInstruction : ILInstruction
	{
		ILInstruction left;
		ILInstruction right;
		
		protected BinaryInstruction(OpCode opCode, ILInstruction left, ILInstruction right)
			: base(opCode)
		{
			this.Left = left;
			this.Right = right;
		}
		
		public ILInstruction Left {
			get { return left; }
			set {
				ValidateArgument(value);
				SetChildInstruction(ref left, value);
			}
		}
		
		public ILInstruction Right {
			get { return right; }
			set {
				ValidateArgument(value);
				SetChildInstruction(ref right, value);
			}
		}
		
		public override void WriteTo(ITextOutput output)
		{
			output.Write(OpCode);
			output.Write('(');
			Left.WriteTo(output);
			output.Write(", ");
			Right.WriteTo(output);
			output.Write(')');
		}
		
		protected override InstructionFlags ComputeFlags()
		{
			return Left.Flags | Right.Flags;
		}
		
		public sealed override TAccumulate AggregateChildren<TSource, TAccumulate>(TAccumulate initial, ILVisitor<TSource> visitor, Func<TAccumulate, TSource, TAccumulate> func)
		{
			TAccumulate value = initial;
			value = func(value, Left.AcceptVisitor(visitor));
			value = func(value, Right.AcceptVisitor(visitor));
			return value;
		}
		
		public sealed override void TransformChildren(ILVisitor<ILInstruction> visitor)
		{
			this.Left = left.AcceptVisitor(visitor);
			this.Right = right.AcceptVisitor(visitor);
		}
		
		internal override ILInstruction Inline(InstructionFlags flagsBefore, Stack<ILInstruction> instructionStack, out bool finished)
		{
			InstructionFlags flagsBeforeRight = flagsBefore | (Left.Flags & ~(InstructionFlags.MayPeek | InstructionFlags.MayPop));
			Right = Right.Inline(flagsBeforeRight, instructionStack, out finished);
			if (finished)
				Left = Left.Inline(flagsBefore, instructionStack, out finished);
			return this;
		}
	}
}

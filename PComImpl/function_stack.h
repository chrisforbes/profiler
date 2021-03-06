#pragma once

class FunctionStack : ThreadPtr< std::stack< bool > >
{
	typedef std::stack< bool > T;

	T * Stack()
	{
		T * t = get();
		if (!t)
			set( t = new T );
		return t;
	}

public:
	FunctionStack()
		: ThreadPtr()
	{
	}

	bool Empty()
	{
		return Stack()->empty();
	}

	bool Peek()
	{
		return Stack()->top();
	}

	void Push( bool value )
	{
		Stack()->push( value );
	}

	bool Pop()
	{
		bool value = Peek();
		Stack()->pop();
		return value;
	}

	bool EmptyOrPeek()
	{
		const std::stack< bool >* t = Stack();
		return t->empty() || t->top();
	}

	bool PopOrEmptyOrPeek()
	{
		std::stack< bool >* t = Stack();
		bool a = t->top();
		t->pop();
		return a || t->empty() || t->top();
	}

	bool EmptyOrPeekThenPush( bool f )
	{
		std::stack< bool >* t = Stack();
		bool result = t->empty() || t->top();
		t->push( f );
		return result;
	}
};


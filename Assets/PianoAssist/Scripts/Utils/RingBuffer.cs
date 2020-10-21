using System.Collections;
using System.Collections.Generic;

public class RingBuffer<T>
{
    private List<T> m_elements;
    private int m_index;

    public RingBuffer()
    {
        m_elements = new List<T>();
        m_index = 0;
    }

    public void AddElement(T element)
    {
        m_elements.Add(element);
    }

    public T GetCurrentElement()
    {
        return m_elements[m_index];
    }

    public T SetCurrentElement(T newElement)
    {
        T oldElement = m_elements[m_index];
        m_elements[m_index] = newElement;
        return oldElement;
    }

    public void Advance()
    {
        m_index++;
        if (m_index >= m_elements.Count)
            m_index = 0;
    }

    public int GetSize()
    {
        return m_elements.Count;
    }

    public void Clear()
    {
        m_elements.Clear();
    }
}

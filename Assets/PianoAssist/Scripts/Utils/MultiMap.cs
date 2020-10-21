using System.Collections;
using System.Collections.Generic;

public class MultiMap<Key, Value>
{
    Dictionary<Key, List<Value>> m_dictionary = new Dictionary<Key, List<Value>>();

    public void Add(Key key, Value value)
    {
        List<Value> list;
        if (m_dictionary.TryGetValue(key, out list))
        {
            list.Add(value);
        }
        else
        {
            list = new List<Value>();
            list.Add(value);
            m_dictionary[key] = list;
        }
    }

    public bool ContainsKey(Key key)
    {
        return m_dictionary.ContainsKey(key);
    }

    public bool Remove(Key key)
    {
        return m_dictionary.Remove(key);
    }

    public void Clear()
    {
        m_dictionary.Clear();
    }

    public IEnumerable<Key> Keys
    {
        get
        {
            return m_dictionary.Keys;
        }
    }

    public List<Value> this[Key key]
    {
        get
        {
            List<Value> list;
            if (!m_dictionary.TryGetValue(key, out list))
            {
                list = new List<Value>();
                m_dictionary[key] = list;
            }
            return list;
        }
    }
}
#nullable enable

using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class BoardSizeInput : MonoBehaviour
{
    private TMP_InputField _inputField = null!;
    private int _lastValidSize = 8;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        if (_inputField == null) throw new ArgumentNullException(nameof(_inputField));
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }
    private void OnDestroy()
    {
        _inputField.onEndEdit.RemoveListener(OnEndEdit);
        OnValueChanged = null;
    }
    private void OnEndEdit(string value)
    {
        if (int.TryParse(value, out int newSize) && newSize >= 2 && newSize <= 4096)
        {
            _lastValidSize = newSize;
            OnValueChanged?.Invoke(newSize);
        }
        else SetValue(_lastValidSize);
    }

    public void SetValue(int size)
    {
        _lastValidSize = size;
        _inputField.text = size.ToString();
    }

    public event Action<int>? OnValueChanged;
}
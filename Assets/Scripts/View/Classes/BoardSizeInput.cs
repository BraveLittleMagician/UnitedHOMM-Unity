#nullable enable

using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class BoardSizeInput : MonoBehaviour
{
    private TMP_InputField _inputField = null!;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        if (_inputField == null) throw new NullReferenceException(nameof(_inputField));
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }
    private void OnDestroy()
    {
        _inputField.onEndEdit.RemoveListener(OnEndEdit);
    }
    private void OnEndEdit(string value)
    {
        if (int.TryParse(value, out int newSize) && newSize >= 2)
            OnValueChanged?.Invoke(newSize);
        else SetValue(int.Parse(_inputField.text));
    }

    public void SetValue(int size)
    {
        _inputField.text = size.ToString();
    }

    public event Action<int>? OnValueChanged;
}
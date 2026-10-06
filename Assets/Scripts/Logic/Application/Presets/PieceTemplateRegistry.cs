#nullable enable

using System;
using System.Collections.Generic;

public sealed class PieceTemplateRegistry
{
    private readonly Dictionary<string, PieceTemplate> _templates;

    public PieceTemplateRegistry(IEnumerable<PieceTemplate> templates)
    {
        if (templates == null)
            throw new ArgumentNullException(nameof(templates));

        _templates = new Dictionary<string, PieceTemplate>(StringComparer.Ordinal);

        foreach (var template in templates)
        {
            if (template == null)
                throw new InvalidOperationException(
                    "В коллекции шаблонов найден null-элемент.");

            if (_templates.ContainsKey(template.Name))
                throw new InvalidOperationException(
                    $"Найден дублирующийся шаблон: '{template.Name}'.");

            _templates[template.Name] = template;
        }

        if (_templates.Count == 0)
            throw new InvalidOperationException(
                "Реестр шаблонов пуст. Добавьте хотя бы одну фигуру.");
    }

    public PieceTemplate Get(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Имя шаблона не может быть пустым", nameof(name));

        if (!_templates.TryGetValue(name, out var template))
            throw new KeyNotFoundException(
                $"Шаблон фигуры '{name}' не найден. " +
                $"Проверьте, что имя есть в StandardPieceTemplates.");

        return template;
    }

    public bool TryGet(string name, out PieceTemplate? template)
    {
        if (string.IsNullOrEmpty(name))
        {
            template = null;
            return false;
        }

        return _templates.TryGetValue(name, out template);
    }

    public bool Contains(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return _templates.ContainsKey(name);
    }
}
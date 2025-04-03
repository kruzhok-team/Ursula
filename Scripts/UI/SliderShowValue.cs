using Godot;
using System;

public partial class SliderShowValue : HSlider
{
    [Export]
    private HSlider _hSlider;
    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");

        // Устанавливаем начальное значение в Label
        UpdateLabelText(_hSlider.Value);
        //UpdateLabelPosition();

        // Подключаем событие ValueChanged
        _hSlider.ValueChanged += OnHSliderValueChanged;
    }

    private void OnHSliderValueChanged(double value)
    {
        UpdateLabelText(value);
        UpdateLabelPosition();
    }

    private void UpdateLabelText(double value)
    {
        _label.Text = value.ToString();
    }

    private async void UpdateLabelPosition()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        float sliderWidth = _hSlider.Size.X - _hSlider.Size.X / 10;
        float sliderMin = (float)_hSlider.MinValue;
        float sliderMax = (float)_hSlider.MaxValue;
        float sliderValue = (float)_hSlider.Value;

        float relativePosition = (sliderValue - sliderMin) / (sliderMax - sliderMin);
        float labelX = relativePosition * sliderWidth;

        _label.Position = new Vector2(labelX - _label.Size.X / 2, _label.Position.Y);
    }
}

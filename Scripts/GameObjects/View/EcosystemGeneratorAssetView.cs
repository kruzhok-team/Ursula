using Fractural.Tasks;
using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using ursula.addons.Ursula.Scripts.GameObjects.Model;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
	public partial class EcosystemGeneratorAssetView : Control
	{
		[Export]
		private Label LabelNameAsset;

		[Export]
		private Button ButtonClickAsset;

		[Export]
		TextureRect PreviewImageRect;

		[Export]
		TextureRect LoadObjectImageRect;

		[Export]
		OptionButton OptionButtonType;

		[Export]
		OptionButton OptionButtonSex;

		[Export]
		SliderShowValue SliderPopulationCount;

		[Export]
		SliderShowValue SliderFamine;

		[Export]
		Control ControlChildCount;

		[Export]
		SliderShowValue SliderShowChildCount;

		public Action<EcosystemGeneratorAssetView> clickItemEvent;

		EcosystemGeneratorAssetInfo _gameObjectAssetInfo;

		public EcosystemGeneratorAssetInfo GameObjectAssetInfo
		{
			get { return _gameObjectAssetInfo; }
		}

		public override void _Ready()
		{
			base._Ready();

			ButtonClickAsset.ButtonDown += OnItemClickEvent;
			OptionButtonType.ItemSelected += OnOptionButtonTypeItemSelected;
			OptionButtonSex.ItemSelected += OnOptionButtonSexItemSelected;
			SliderPopulationCount.ValueChanged += OnSliderPopulationCountValueChanged;
			SliderFamine.ValueChanged += OnSliderFamineValueChanged;
			SliderShowChildCount.ValueChanged += OnSliderShowChildCountValueChanged;

            ControlChildCount.Visible = false;
		}

		public void LoadDefaultValues()
		{
            SliderPopulationCount.Value = _gameObjectAssetInfo.PopulationCount;
            SliderFamine.Value = _gameObjectAssetInfo.Famine;
            SliderShowChildCount.Value = _gameObjectAssetInfo.ChildCount;
        }

		public override void _ExitTree()
		{
			base._ExitTree();
			ButtonClickAsset.ButtonDown -= OnItemClickEvent;
		}

		public async void Invalidate(GameObjectAssetInfo assetInfo)
		{
			PreviewImageRect.Visible = false;
			if (assetInfo != null)
			{
				_gameObjectAssetInfo = new EcosystemGeneratorAssetInfo(assetInfo);
				LabelNameAsset.Text = _gameObjectAssetInfo.Name;

				PreviewImageRect.Texture = await _gameObjectAssetInfo.GetPreviewImage();

				PreviewImageRect.Visible = PreviewImageRect.Texture != null;

				LoadObjectImageRect.Visible = false;
			}
			else
			{
				LabelNameAsset.Visible = false;
				LoadObjectImageRect.Visible = true;
			}
		}

		async GDTask LoadPreviewImage(string path)
		{
			PreviewImageRect.Texture = await _LoadPreviewImage(path);
		}

		private async GDTask<Texture2D> _LoadPreviewImage(string path)
		{
			Texture2D tex;
			Image img = new Image();

			var err = await Task.Run(() => img.Load(path));

			if (err != Error.Ok)
			{
				GD.Print("Failed to load image from path: " + path);
			}
			else
			{
				tex = ImageTexture.CreateFromImage(img);
				return tex;
			}
			return null;
		}

		private void OnItemClickEvent()
		{
			clickItemEvent?.Invoke(this);
		}

		private void OnOptionButtonTypeItemSelected(long index)
		{
			if (_gameObjectAssetInfo != null)
			{
				_gameObjectAssetInfo.Type = OptionButtonType.GetItemText((int)index);
			}
		}

		private void OnSliderShowChildCountValueChanged(double value)
		{
			if (_gameObjectAssetInfo != null)
			{
				_gameObjectAssetInfo.ChildCount = (int)Math.Round(SliderShowChildCount.Value);
			}
		}

		private void OnSliderPopulationCountValueChanged(double value)
		{
			if (_gameObjectAssetInfo != null)
			{
				_gameObjectAssetInfo.PopulationCount = (int)Math.Round(SliderPopulationCount.Value);
			}
		}

		private void OnSliderFamineValueChanged(double value)
		{
			if (_gameObjectAssetInfo != null)
			{
				_gameObjectAssetInfo.Famine = (int)Math.Round(SliderFamine.Value);
			}
		}

		private void OnOptionButtonSexItemSelected(long index)
		{
			if (_gameObjectAssetInfo == null)
			{
				return;
			}
			_gameObjectAssetInfo.Sex = OptionButtonSex.GetItemText((int)index);

			if (_gameObjectAssetInfo.Sex == "Женский")
			{
				ControlChildCount.Visible = true;
			}
			else
			{
				ControlChildCount.Visible = false;
			}
		}

		public void SetSex(bool isFemale)
		{
			if (isFemale)
			{
                OptionButtonSex.Select(1);
                OnOptionButtonSexItemSelected(1);
            }
			else
			{
                OptionButtonSex.Select(0);
                OnOptionButtonSexItemSelected(0);
			}
        }

		public void SetType(bool isHunter)
		{
			if (isHunter)
			{
				OptionButtonType.Select(0);
                OnOptionButtonTypeItemSelected(0);
            }
			else
			{
                OptionButtonType.Select(1);
                OnOptionButtonTypeItemSelected(1);
            }
		}

		public void OnDependenciesInjected()
		{
			//throw new NotImplementedException();
		}
	}
}

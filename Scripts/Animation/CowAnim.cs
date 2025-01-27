using Godot;
using System;

public partial class CowAnim : BaseAnimation
{   

    public override async void PlayUseAnimation()
    {
        State = state.Use;
        PlayEatingAnimation();
        //PlayHeadSpinAnimation();
    }


}

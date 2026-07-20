using BasicWpfLibrary;
using ImageCalculator;
using System;

namespace Fractal3d;

public class BranchVm : ViewModelBase
{
    private readonly LSystemBranch _branch;

    public BranchVm(LSystemBranch branch)
    {
        _branch = branch;
    }

    public LSystemBranch Branch
    {
        get => (LSystemBranch)_branch.Clone();
    }

    public float RelativePositionX
    {
        get => _branch.RelativePostionX;
        set
        {
            if (Math.Abs(value - _branch.RelativePostionX) < 1e-6)
                return;
            _branch.RelativePostionX = value;
            OnPropertyChanged();
        }
    }

    public float Attenuation
    {
        get => _branch.Attenuation;
        set
        {
            if (Math.Abs(value - _branch.Attenuation) < 1e-6)
                return;
            _branch.Attenuation = value;
            OnPropertyChanged();
        }
    }

    public float AngleXDegrees
    {
        get => _branch.AngleXDegrees;
        set
        {
            if (Math.Abs(value - _branch.AngleXDegrees) < 1e-6)
                return;
            _branch.AngleXDegrees = value;
            OnPropertyChanged();
        }
    }

    public float AngleYDegrees
    {
        get => _branch.AngleYDegrees;
        set
        {
            if (Math.Abs(value - _branch.AngleYDegrees) < 1e-6)
                return;
            _branch.AngleYDegrees = value;
            OnPropertyChanged();
        }
    }

    public float AngleZDegrees
    {
        get => _branch.AngleZDegrees;
        set
        {
            if (Math.Abs(value - _branch.AngleZDegrees) < 1e-6)
                return;
            _branch.AngleZDegrees = value;
            OnPropertyChanged();
        }
    }
}


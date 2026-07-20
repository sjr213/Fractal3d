using ImageCalculator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fractal3d;

internal static class BranchUtil
{
    public static ObservableCollection<BranchVm> WrapBranches(List<LSystemBranch> branches)
    {
        return new ObservableCollection<BranchVm>(branches.Select(b => new BranchVm(b)));
    }

    public static List<LSystemBranch> UnwrapBranches(ObservableCollection<BranchVm> branchVms)
    {
        return branchVms.Select(vm => vm.Branch).ToList();
    }

    public static List<LSystemBranch> MakeDefaultBranches()
    {
        return new List<LSystemBranch>
        {
            new LSystemBranch(0.5f, 1.0f, 0.0f, 0.0f, 0.30f),
         //   new LSystemBranch(0.75f, 1.0f, 0.0f, 0.0f, -0.3f),
         //   new LSystemBranch(1.0f, 1.0f, 10.0f, 10.0f, 0.2f)
        };
    }
}

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

}

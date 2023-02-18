namespace Fractal3d;

using FractureCommonLib;

public interface IColorPointVmParent
{
    void UpdateRectItem(ColorPoint cp, int index);

    bool CanAdd(double colorPosition);

    void AddRectItem(ColorPoint cp);

    void DeleteRectItem(int index);
}

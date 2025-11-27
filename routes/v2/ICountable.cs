namespace routes.v2;

public interface ICountable<T>
{
    T GetNext();
    T GetPrevious();
}

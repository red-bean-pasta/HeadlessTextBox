namespace MonoTextBox.Utils;

public interface IBranch<T> where T : IBranch<T>
{
    int Length { get; }
    
    (T, T) Split(int index);
}
namespace MonoTextBox.Utils;

public interface IBranch<TSelf> where TSelf : IBranch<TSelf>
{
    int Length { get; }
    
    (TSelf, TSelf) Split(int index);
}
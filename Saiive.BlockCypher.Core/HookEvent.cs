namespace Saiive.BlockCypher.Core
{
    public enum HookEvent {
        UnconfirmedTransaction,
        NewBlock,
        ConfirmedTransaction,
        TransactionConfirmation,
        DoubleSpendTransaction
    }
}

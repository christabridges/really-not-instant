[Serializable]
internal class InvalidOriginException : Exception {
    public InvalidOriginException() {
    }

    public InvalidOriginException(string? message) : base(message) {
    }

    public InvalidOriginException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
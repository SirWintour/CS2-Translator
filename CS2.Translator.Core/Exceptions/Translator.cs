namespace CS2.Translator.Core.Exceptions;
public class TranslatorException(string message) : Exception(message);

public class NoInternetException() : TranslatorException("No internet connection");

public class GoogleTranslateTimeoutException() : TranslatorException("Rate limited by Google");
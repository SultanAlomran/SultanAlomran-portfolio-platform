namespace Portfolio.Domain.Enums;

public enum ContentStatus : byte { Draft = 0, Published = 1, Archived = 2 }
public enum DifficultyLevel : byte { Beginner = 1, Intermediate = 2, Advanced = 3 }
public enum ContactStatus : byte { New = 0, Read = 1, Archived = 2, InProgress = 1, Closed = 2 }
public enum InteractionType : byte { View = 0, Download = 1, Share = 2 }
public enum NotificationType : byte { Info = 0, Success = 1, Warning = 2, Error = 3, System = 4 }

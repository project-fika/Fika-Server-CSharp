export interface QuestObjective {
    description: string;
}

export interface QuestData {
    name: string;
    description: string;
    objectives: QuestObjective[];
}

export interface QuestSearchResultDto {
    templateId: string;
    name: string;
}

export const EQuestState = {
    Started: 0,
    InProgress: 1,
    Completed: 2,
} as const;

export type EQuestState = (typeof EQuestState)[keyof typeof EQuestState];

export interface DetailedQuestObjective {
    description: string;
    progress: number;
    target: number;
    state: EQuestState;
}

export interface DetailedQuestData {
    name: string;
    description: string;
    completed: boolean;
    objectives: DetailedQuestObjective[];
}

export interface QuestObjective {
    description: string;
}

export interface QuestData {
    name: string;
    description: string;
    objectives: QuestObjective[];
}

export interface QuestSearchResultDto {
    templateId: string;
    name: string;
}

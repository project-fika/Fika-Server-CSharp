export const EQuestState = {
    Started: 0,
    InProgress: 1,
    Completed: 2,
} as const;

export type EQuestState = (typeof EQuestState)[keyof typeof EQuestState];

export interface QuestObjective {
    id?: string;
    description?: string;
    progress?: number;
    target?: number;
    state?: EQuestState;
}

export interface ItemReward {
    amount?: number | null;
    itemId: string;
}

export interface TraderReward {
    amount?: number | null;
    traderId: string;
}

export interface ExperienceReward {
    amount?: number | null;
}

export interface QuestData {
    id?: string;
    name?: string;
    description?: string;
    completed?: boolean;
    objectives?: QuestObjective[];
    itemRewards?: ItemReward[] | null;
    traderRewards?: TraderReward[] | null;
    experienceRewards?: ExperienceReward[] | null;
}

export interface QuestSearchResultDto {
    templateId: string;
    name: string;
}

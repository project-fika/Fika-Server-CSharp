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

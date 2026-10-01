export interface SendItemModel {
    itemName: string;
    templateId: string;
    amount: number;
    message: string;
    expirationDays: number;
    foundInRaid: boolean;
    useDate: boolean;
    date: Date | null;
}

export interface ItemSearchResultDto {
    templateId: string;
    name: string;
}

export interface ResolvedItemDto {
    templateId: string;
    name: string;
    description: string;
    maxItems: number;
}

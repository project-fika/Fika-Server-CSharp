interface SingleTimerDto {
    ticks: number;
    profileId: string;
    itemTemplate: string;
    itemName: string;
    amount: number;
    message: string;
    foundInRaid: boolean;
    sendDate: string;
}

interface AllTimerDto {
    ticks: number;
    itemTemplate: string;
    itemName: string;
    amount: number;
    message: string;
    foundInRaid: boolean;
    profileIds: string[];
    sendDate: string;
}

export interface QueuedItemsResponse {
    singleTimers: SingleTimerDto[];
    allTimers: AllTimerDto[];
}

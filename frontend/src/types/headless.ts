export const EHeadlessState = {
    Ready: 0,
    NotReady: 1,
} as const;

export type EHeadlessState = (typeof EHeadlessState)[keyof typeof EHeadlessState];

export interface OnlineHeadless {
    profileId: string;
    nickname: string;
    state: EHeadlessState;
    players: number;
}

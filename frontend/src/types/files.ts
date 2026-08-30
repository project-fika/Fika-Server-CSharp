export interface FileTreeNode {
    value: string;
    text: string;
    isDirectory: boolean;
    endText?: string;
    children?: FileTreeNode[];
}

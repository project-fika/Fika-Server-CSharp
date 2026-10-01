import { Anchor, List, Stack, Text, Title } from '@mantine/core';

export function AboutPage() {
    const currentYear = new Date().getFullYear();

    return (
        <Stack gap="md">
            <Title order={1}>About</Title>

            <List
                spacing="sm"
                icon={
                    <Text size="sm" c="dimmed">
                        -
                    </Text>
                }
            >
                <List.Item>
                    Built with ❤️ using{' '}
                    <Anchor href="https://mantine.dev" target="_blank" rel="noopener noreferrer">
                        Mantine UI
                    </Anchor>
                    , powered by{' '}
                    <Anchor href="https://react.dev" target="_blank" rel="noopener noreferrer">
                        React
                    </Anchor>
                    .
                </List.Item>

                <List.Item>
                    Icons and illustrations courtesy of{' '}
                    <Anchor href="https://tabler.io/icons" target="_blank" rel="noopener noreferrer">
                        Tabler Icons
                    </Anchor>{' '}
                    and{' '}
                    <Anchor href="https://undraw.co" target="_blank" rel="noopener noreferrer">
                        unDraw
                    </Anchor>
                    .
                </List.Item>

                <List.Item>
                    All maps images are taken from the{' '}
                    <Anchor href="https://escapefromtarkov.fandom.com/wiki/Escape_from_Tarkov_Wiki" target="_blank" rel="noopener noreferrer">
                        Tarkov Fandom Wiki
                    </Anchor>{' '}
                    (
                    <Anchor href="https://creativecommons.org/licenses/by-nc-sa/3.0/" target="_blank" rel="noopener noreferrer">
                        CC BY-NC-SA
                    </Anchor>
                    , no changes have been made).
                </List.Item>

                <List.Item>
                    All item links and icons are from{' '}
                    <Anchor href="https://tarkov.dev/api/" target="_blank" rel="noopener noreferrer">
                        Tarkov.dev
                    </Anchor>
                    .
                </List.Item>

                <List.Item>© {currentYear} Project Fika. All rights reserved.</List.Item>
            </List>
        </Stack>
    );
}

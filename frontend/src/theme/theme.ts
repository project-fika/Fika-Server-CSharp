import { Badge, Button, createTheme } from '@mantine/core';

export const fikaTheme = createTheme({
    colors: {
        fikaDark: [
            '#1e1e1e', // 0: App background
            '#232323', // 1: Card / Container bg
            '#282828', // 2: Surface bg
            '#2d2d2d', // 3: Element bg
            '#323232', // 4: Muted border / bg
            '#807054', // 5: Primary accent (Buttons, Headers)
            '#6b5d45', // 6: Primary Hover
            '#564a37', // 7: Primary Active
            '#403729', // 8: Darker accent
            '#2b241b', // 9: Darkest
        ],
    },
    primaryColor: 'fikaDark',
    primaryShade: 5,
    fontFamily: 'bender, sans-serif',
    components: {
        Button: Button.extend({
            vars: (_theme, props) => {
                if (!props.variant || props.variant === 'filled') {
                    return {
                        root: {
                            '--button-color': '#f5efe6',
                            '--button-bg': '#807054',
                            '--button-hover': '#6b5d45',
                        },
                    };
                }

                if (props.variant === 'default') {
                    return {
                        root: {
                            '--button-bg': '#383838',
                            '--button-color': '#d1c7b7',
                            '--button-bd': '1px solid #4a4a4a',
                            '--button-hover': '#444444',
                        },
                    };
                }

                if (props.variant === 'light') {
                    return {
                        root: {
                            '--button-bg': 'rgba(128, 112, 84, 0.15)',
                            '--button-color': '#9a8866',
                            '--button-hover': 'rgba(128, 112, 84, 0.25)',
                        },
                    };
                }

                return { root: {} };
            },
        }),
        Badge: Badge.extend({
            vars: (_theme, props) => {
                // If an explicit color prop is passed (e.g., color="green"), defer to Mantine's color engine
                if (props.color && props.color !== 'fikaDark') {
                    return { root: {} };
                }

                if (!props.variant || props.variant === 'filled') {
                    return {
                        root: {
                            '--badge-color': '#f5efe6',
                            '--badge-bg': '#807054',
                            '--badge-bd': '1px solid #99815e',
                        },
                    };
                }

                if (props.variant === 'light') {
                    return {
                        root: {
                            '--badge-bg': 'rgba(128, 112, 84, 0.2)',
                            '--badge-color': '#9a8866',
                            '--badge-bd': '1px solid rgba(224, 172, 105, 0.3)',
                        },
                    };
                }

                if (props.variant === 'outline') {
                    return {
                        root: {
                            '--badge-color': '#9a8866',
                            '--badge-bd': '1px solid #807054',
                        },
                    };
                }

                return { root: {} };
            },
        }),
        Notification: {
            styles: {
                title: {
                    color: '#c7c5b3',
                },
                description: {
                    color: '#c7c5b3',
                },
            },
        },
    },
});

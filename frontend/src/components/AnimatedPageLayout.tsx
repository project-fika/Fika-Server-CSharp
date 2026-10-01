import { Box } from '@mantine/core';
import { AnimatePresence, motion } from 'framer-motion';
import { useLocation, useOutlet } from 'react-router-dom';

export function AnimatedPageLayout() {
    const location = useLocation();
    const outlet = useOutlet();

    return (
        <AnimatePresence mode="popLayout" initial={false}>
            <motion.div key={location.pathname} initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -6 }} transition={{ duration: 0.15, ease: 'easeOut' }}>
                <Box style={{ width: '100%', minHeight: '100%' }}>{outlet}</Box>
            </motion.div>
        </AnimatePresence>
    );
}

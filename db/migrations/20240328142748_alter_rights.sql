-- migrate:up
GRANT ALL ON TABLE public.events_01_counter TO safe;
GRANT ALL ON TABLE public.snapshots_01_counter TO safe;
GRANT ALL ON SEQUENCE public.snapshots_01_counter_id_seq TO safe;
GRANT ALL ON SEQUENCE public.events_01_counter_id_seq TO safe;


-- migrate:down


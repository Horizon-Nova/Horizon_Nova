BEGIN;

CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- -----------------------------------------------------------------------------
-- 1) Wardrobe domain
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS ww_wardrobe_items
(
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id         text UNIQUE,
    item_type           text NOT NULL DEFAULT 'wardrobe_item',
    category_key        text NOT NULL,
    label               text NOT NULL,
    image_url           text NOT NULL,
    source_image_url    text NOT NULL,
    cropped_image_url   text NOT NULL,
    generated_image_url text,
    score               real,
    is_detected         boolean NOT NULL DEFAULT false,
    is_generated        boolean NOT NULL DEFAULT false,
    metadata            jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    updated_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT ck_ww_wardrobe_items_item_type_non_empty CHECK (btrim(item_type) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_category_key_non_empty CHECK (btrim(category_key) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_label_non_empty CHECK (btrim(label) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_image_url_non_empty CHECK (btrim(image_url) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_source_image_url_non_empty CHECK (btrim(source_image_url) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_cropped_image_url_non_empty CHECK (btrim(cropped_image_url) <> ''),
    CONSTRAINT ck_ww_wardrobe_items_score_range CHECK (score IS NULL OR (score >= 0 AND score <= 1))
);

CREATE TABLE IF NOT EXISTS ww_wardrobe_embeddings
(
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    wardrobe_item_id uuid NOT NULL REFERENCES ww_wardrobe_items (id) ON DELETE CASCADE,
    embedding_type   text NOT NULL DEFAULT 'clip_vit_b_32',
    model_name       text,
    embedding        vector(512) NOT NULL,
    created_at       timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT uq_ww_wardrobe_embeddings_item_type UNIQUE (wardrobe_item_id, embedding_type),
    CONSTRAINT ck_ww_wardrobe_embeddings_embedding_type_non_empty CHECK (btrim(embedding_type) <> ''),
    CONSTRAINT ck_ww_wardrobe_embeddings_vector_dims CHECK (vector_dims(embedding) = 512)
);

-- -----------------------------------------------------------------------------
-- 2) Outfit domain
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS ww_outfits
(
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id         text UNIQUE,
    source_type         text NOT NULL DEFAULT 'generate',
    name                text NOT NULL,
    occasion            text NOT NULL,
    weather_summary     text NOT NULL,
    reason              text NOT NULL DEFAULT '',
    generated_image_url text,
    metadata            jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    updated_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT ck_ww_outfits_source_type CHECK (source_type IN ('generate', 'tweak', 'manual')),
    CONSTRAINT ck_ww_outfits_name_non_empty CHECK (btrim(name) <> ''),
    CONSTRAINT ck_ww_outfits_occasion_non_empty CHECK (btrim(occasion) <> ''),
    CONSTRAINT ck_ww_outfits_weather_summary_non_empty CHECK (btrim(weather_summary) <> '')
);

CREATE TABLE IF NOT EXISTS ww_outfit_items
(
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    outfit_id        uuid NOT NULL REFERENCES ww_outfits (id) ON DELETE CASCADE,
    wardrobe_item_id uuid NOT NULL REFERENCES ww_wardrobe_items (id) ON DELETE CASCADE,
    item_order       smallint NOT NULL DEFAULT 0,
    created_at       timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT uq_ww_outfit_items_outfit_item UNIQUE (outfit_id, wardrobe_item_id),
    CONSTRAINT uq_ww_outfit_items_outfit_order UNIQUE (outfit_id, item_order),
    CONSTRAINT ck_ww_outfit_items_item_order_non_negative CHECK (item_order >= 0)
);

-- -----------------------------------------------------------------------------
-- 3) Look photo domain
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS ww_look_photos
(
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id     text UNIQUE,
    outfit_id       uuid REFERENCES ww_outfits (id) ON DELETE CASCADE,
    image_url       text NOT NULL,
    occasion        text NOT NULL DEFAULT '',
    weather_summary text NOT NULL DEFAULT '',
    created_at      timestamptz NOT NULL DEFAULT timezone('utc', now()),
    updated_at      timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT ck_ww_look_photos_image_url_non_empty CHECK (btrim(image_url) <> '')
);

CREATE TABLE IF NOT EXISTS ww_look_photo_embeddings
(
    id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    look_photo_id  uuid NOT NULL REFERENCES ww_look_photos (id) ON DELETE CASCADE,
    embedding_type text NOT NULL DEFAULT 'clip_vit_b_32',
    model_name     text,
    embedding      vector(512) NOT NULL,
    created_at     timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT uq_ww_look_photo_embeddings_photo_type UNIQUE (look_photo_id, embedding_type),
    CONSTRAINT ck_ww_look_photo_embeddings_embedding_type_non_empty CHECK (btrim(embedding_type) <> ''),
    CONSTRAINT ck_ww_look_photo_embeddings_vector_dims CHECK (vector_dims(embedding) = 512)
);

-- -----------------------------------------------------------------------------
-- 4) Future plans
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS ww_future_plans
(
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_date  date NOT NULL UNIQUE,
    note       text NOT NULL,
    saved_at   timestamptz NOT NULL DEFAULT timezone('utc', now()),
    created_at timestamptz NOT NULL DEFAULT timezone('utc', now()),
    updated_at timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT ck_ww_future_plans_note_non_empty CHECK (btrim(note) <> ''),
    CONSTRAINT ck_ww_future_plans_plan_date_reasonable CHECK (plan_date >= DATE '2000-01-01' AND plan_date <= DATE '2100-12-31')
);

-- -----------------------------------------------------------------------------
-- 5) Calendar looks
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS ww_calendar_looks
(
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id         text UNIQUE,
    outfit_id           uuid REFERENCES ww_outfits (id) ON DELETE CASCADE,
    look_date           date NOT NULL,
    name                text NOT NULL,
    occasion            text NOT NULL,
    weather_summary     text NOT NULL,
    generated_image_url text,
    saved_at            timestamptz NOT NULL DEFAULT timezone('utc', now()),
    created_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    updated_at          timestamptz NOT NULL DEFAULT timezone('utc', now()),
    metadata            jsonb NOT NULL DEFAULT '{}'::jsonb,
    CONSTRAINT ck_ww_calendar_looks_name_non_empty CHECK (btrim(name) <> ''),
    CONSTRAINT ck_ww_calendar_looks_occasion_non_empty CHECK (btrim(occasion) <> ''),
    CONSTRAINT ck_ww_calendar_looks_weather_summary_non_empty CHECK (btrim(weather_summary) <> ''),
    CONSTRAINT ck_ww_calendar_looks_look_date_reasonable CHECK (look_date >= DATE '2000-01-01' AND look_date <= DATE '2100-12-31')
);

CREATE TABLE IF NOT EXISTS ww_calendar_look_items
(
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    calendar_look_id  uuid NOT NULL REFERENCES ww_calendar_looks (id) ON DELETE CASCADE,
    wardrobe_item_id  uuid REFERENCES ww_wardrobe_items (id) ON DELETE CASCADE,
    item_label        text,
    item_order        smallint NOT NULL DEFAULT 0,
    created_at        timestamptz NOT NULL DEFAULT timezone('utc', now()),
    CONSTRAINT uq_ww_calendar_look_items_look_order UNIQUE (calendar_look_id, item_order),
    CONSTRAINT ck_ww_calendar_look_items_item_order_non_negative CHECK (item_order >= 0),
    CONSTRAINT ck_ww_calendar_look_items_ref_or_label CHECK (wardrobe_item_id IS NOT NULL OR (item_label IS NOT NULL AND btrim(item_label) <> ''))
);

-- -----------------------------------------------------------------------------
-- 6) Indexes
-- -----------------------------------------------------------------------------

CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_items_created_at ON ww_wardrobe_items (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_items_category_key ON ww_wardrobe_items (category_key);
CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_items_item_type ON ww_wardrobe_items (item_type);

CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_embeddings_item_id ON ww_wardrobe_embeddings (wardrobe_item_id);
CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_embeddings_created_at ON ww_wardrobe_embeddings (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_embeddings_type ON ww_wardrobe_embeddings (embedding_type);
CREATE INDEX IF NOT EXISTS idx_ww_wardrobe_embeddings_hnsw ON ww_wardrobe_embeddings USING hnsw (embedding vector_cosine_ops) WITH (m = 16, ef_construction = 64);

CREATE INDEX IF NOT EXISTS idx_ww_outfits_created_at ON ww_outfits (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_outfits_source_type ON ww_outfits (source_type);
CREATE INDEX IF NOT EXISTS idx_ww_outfits_occasion ON ww_outfits (occasion);

CREATE INDEX IF NOT EXISTS idx_ww_outfit_items_outfit_id ON ww_outfit_items (outfit_id);
CREATE INDEX IF NOT EXISTS idx_ww_outfit_items_item_id ON ww_outfit_items (wardrobe_item_id);

CREATE INDEX IF NOT EXISTS idx_ww_look_photos_created_at ON ww_look_photos (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_look_photos_outfit_id ON ww_look_photos (outfit_id);

CREATE INDEX IF NOT EXISTS idx_ww_look_photo_embeddings_photo_id ON ww_look_photo_embeddings (look_photo_id);
CREATE INDEX IF NOT EXISTS idx_ww_look_photo_embeddings_created_at ON ww_look_photo_embeddings (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_look_photo_embeddings_type ON ww_look_photo_embeddings (embedding_type);
CREATE INDEX IF NOT EXISTS idx_ww_look_photo_embeddings_hnsw ON ww_look_photo_embeddings USING hnsw (embedding vector_cosine_ops) WITH (m = 16, ef_construction = 64);

CREATE INDEX IF NOT EXISTS idx_ww_future_plans_plan_date ON ww_future_plans (plan_date DESC);
CREATE INDEX IF NOT EXISTS idx_ww_future_plans_saved_at ON ww_future_plans (saved_at DESC);

CREATE INDEX IF NOT EXISTS idx_ww_calendar_looks_look_date ON ww_calendar_looks (look_date DESC);
CREATE INDEX IF NOT EXISTS idx_ww_calendar_looks_saved_at ON ww_calendar_looks (saved_at DESC);
CREATE INDEX IF NOT EXISTS idx_ww_calendar_looks_outfit_id ON ww_calendar_looks (outfit_id);

CREATE INDEX IF NOT EXISTS idx_ww_calendar_look_items_look_id ON ww_calendar_look_items (calendar_look_id);
CREATE INDEX IF NOT EXISTS idx_ww_calendar_look_items_item_id ON ww_calendar_look_items (wardrobe_item_id);

-- -----------------------------------------------------------------------------
-- 7) Auto-update updated_at
-- -----------------------------------------------------------------------------

CREATE OR REPLACE FUNCTION ww_set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS
$$
BEGIN
    NEW.updated_at := timezone('utc', now());
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_ww_wardrobe_items_updated_at ON ww_wardrobe_items;
CREATE TRIGGER trg_ww_wardrobe_items_updated_at
BEFORE UPDATE ON ww_wardrobe_items
FOR EACH ROW
EXECUTE FUNCTION ww_set_updated_at();

DROP TRIGGER IF EXISTS trg_ww_outfits_updated_at ON ww_outfits;
CREATE TRIGGER trg_ww_outfits_updated_at
BEFORE UPDATE ON ww_outfits
FOR EACH ROW
EXECUTE FUNCTION ww_set_updated_at();

DROP TRIGGER IF EXISTS trg_ww_look_photos_updated_at ON ww_look_photos;
CREATE TRIGGER trg_ww_look_photos_updated_at
BEFORE UPDATE ON ww_look_photos
FOR EACH ROW
EXECUTE FUNCTION ww_set_updated_at();

DROP TRIGGER IF EXISTS trg_ww_future_plans_updated_at ON ww_future_plans;
CREATE TRIGGER trg_ww_future_plans_updated_at
BEFORE UPDATE ON ww_future_plans
FOR EACH ROW
EXECUTE FUNCTION ww_set_updated_at();

DROP TRIGGER IF EXISTS trg_ww_calendar_looks_updated_at ON ww_calendar_looks;
CREATE TRIGGER trg_ww_calendar_looks_updated_at
BEFORE UPDATE ON ww_calendar_looks
FOR EACH ROW
EXECUTE FUNCTION ww_set_updated_at();

COMMIT;

-- =============================================================================
-- Sample queries
-- =============================================================================
-- Notes:
-- - Replace bind placeholders with your driver style ($1, :name, etc.).
-- - Query vectors must be vector(512).
/*

-- -----------------------------------------------------------------------------
-- A) ImportImages: upsert wardrobe item + upsert embedding
-- -----------------------------------------------------------------------------
-- Inputs:
--   :external_id, :category_key, :label, :image_url, :source_image_url, :cropped_image_url,
--   :generated_image_url, :score, :is_detected, :is_generated, :metadata_jsonb, :embedding_vector
WITH upsert_item AS
(
    INSERT INTO ww_wardrobe_items
    (
        external_id, category_key, label, image_url, source_image_url, cropped_image_url,
        generated_image_url, score, is_detected, is_generated, metadata
    )
    VALUES
    (
        :external_id, :category_key, :label, :image_url, :source_image_url, :cropped_image_url,
        :generated_image_url, :score, :is_detected, :is_generated, COALESCE(:metadata_jsonb, '{}'::jsonb)
    )
    ON CONFLICT (external_id) DO UPDATE
    SET category_key        = EXCLUDED.category_key,
        label               = EXCLUDED.label,
        image_url           = EXCLUDED.image_url,
        source_image_url    = EXCLUDED.source_image_url,
        cropped_image_url   = EXCLUDED.cropped_image_url,
        generated_image_url = EXCLUDED.generated_image_url,
        score               = EXCLUDED.score,
        is_detected         = EXCLUDED.is_detected,
        is_generated        = EXCLUDED.is_generated,
        metadata            = EXCLUDED.metadata
    RETURNING id
)
INSERT INTO ww_wardrobe_embeddings (wardrobe_item_id, embedding_type, model_name, embedding)
SELECT id, 'clip_vit_b_32', 'CLIP_ViT_B_32', :embedding_vector::vector(512)
FROM upsert_item
ON CONFLICT (wardrobe_item_id, embedding_type) DO UPDATE
SET embedding  = EXCLUDED.embedding,
    model_name = EXCLUDED.model_name,
    created_at = timezone('utc', now());

-- -----------------------------------------------------------------------------
-- B) GenerateOutfit/TweakOutfit: nearest wardrobe items by cosine distance
-- -----------------------------------------------------------------------------
-- Inputs: :query_vector, :limit
SELECT
    wi.id,
    wi.category_key,
    wi.label,
    wi.image_url,
    wi.cropped_image_url,
    wi.generated_image_url,
    1 - (we.embedding <=> :query_vector::vector(512)) AS cosine_similarity
FROM ww_wardrobe_embeddings AS we
JOIN ww_wardrobe_items AS wi ON wi.id = we.wardrobe_item_id
WHERE wi.item_type = 'wardrobe_item'
ORDER BY we.embedding <=> :query_vector::vector(512)
LIMIT :limit;

-- -----------------------------------------------------------------------------
-- C) OutfitHistory: latest N outfits
-- -----------------------------------------------------------------------------
-- Input: :limit
SELECT
    o.id,
    o.name,
    o.occasion,
    o.weather_summary,
    o.generated_image_url,
    o.created_at,
    COALESCE(string_agg(wi.label, ', ' ORDER BY oi.item_order), '') AS item_labels
FROM ww_outfits AS o
LEFT JOIN ww_outfit_items AS oi ON oi.outfit_id = o.id
LEFT JOIN ww_wardrobe_items AS wi ON wi.id = oi.wardrobe_item_id
GROUP BY o.id
ORDER BY o.created_at DESC
LIMIT :limit;

-- -----------------------------------------------------------------------------
-- D1) SaveFuturePlan: upsert by plan_date
-- -----------------------------------------------------------------------------
-- Inputs: :plan_date, :note
INSERT INTO ww_future_plans (plan_date, note, saved_at)
VALUES (:plan_date::date, :note, timezone('utc', now()))
ON CONFLICT (plan_date) DO UPDATE
SET note     = EXCLUDED.note,
    saved_at = timezone('utc', now())
RETURNING id, plan_date, note, saved_at;

-- -----------------------------------------------------------------------------
-- D2) FuturePlans: list plans
-- -----------------------------------------------------------------------------
SELECT id, plan_date, note, saved_at
FROM ww_future_plans
ORDER BY plan_date ASC;

-- -----------------------------------------------------------------------------
-- E1) SaveOutfitLook: create look and optional label items
-- -----------------------------------------------------------------------------
-- Inputs: :external_id, :outfit_id, :look_date, :name, :occasion, :weather_summary, :generated_image_url
INSERT INTO ww_calendar_looks
(
    external_id, outfit_id, look_date, name, occasion, weather_summary, generated_image_url, saved_at
)
VALUES
(
    :external_id, :outfit_id::uuid, :look_date::date, :name, :occasion, :weather_summary, :generated_image_url, timezone('utc', now())
)
RETURNING id;

-- Example for inserting label list rows after getting :calendar_look_id:
-- INSERT INTO ww_calendar_look_items (calendar_look_id, item_label, item_order)
-- VALUES (:calendar_look_id::uuid, :item_label, :item_order);

-- -----------------------------------------------------------------------------
-- E2) OutfitLooks by date
-- -----------------------------------------------------------------------------
-- Input: :look_date
SELECT
    cl.id,
    cl.external_id,
    cl.look_date,
    cl.name,
    cl.occasion,
    cl.weather_summary,
    cl.generated_image_url,
    cl.saved_at,
    COALESCE(
        json_agg(
            json_build_object(
                'item_order', cli.item_order,
                'wardrobe_item_id', cli.wardrobe_item_id,
                'item_label', COALESCE(wi.label, cli.item_label)
            )
            ORDER BY cli.item_order
        ) FILTER (WHERE cli.id IS NOT NULL),
        '[]'::json
    ) AS items
FROM ww_calendar_looks AS cl
LEFT JOIN ww_calendar_look_items AS cli ON cli.calendar_look_id = cl.id
LEFT JOIN ww_wardrobe_items AS wi ON wi.id = cli.wardrobe_item_id
WHERE cl.look_date = :look_date::date
GROUP BY cl.id
ORDER BY cl.saved_at DESC;

-- -----------------------------------------------------------------------------
-- E3) OutfitLookDates: distinct dates with saved looks
-- -----------------------------------------------------------------------------
SELECT DISTINCT look_date
FROM ww_calendar_looks
ORDER BY look_date DESC;

-- -----------------------------------------------------------------------------
-- F1) SaveLookPhoto: insert photo
-- -----------------------------------------------------------------------------
-- Inputs: :external_id, :outfit_id, :image_url, :occasion, :weather_summary
INSERT INTO ww_look_photos (external_id, outfit_id, image_url, occasion, weather_summary)
VALUES (:external_id, :outfit_id::uuid, :image_url, :occasion, :weather_summary)
RETURNING id;

-- -----------------------------------------------------------------------------
-- F2) SaveLookPhoto embedding upsert
-- -----------------------------------------------------------------------------
-- Inputs: :look_photo_id, :embedding_vector
INSERT INTO ww_look_photo_embeddings (look_photo_id, embedding_type, model_name, embedding)
VALUES (:look_photo_id::uuid, 'clip_vit_b_32', 'CLIP_ViT_B_32', :embedding_vector::vector(512))
ON CONFLICT (look_photo_id, embedding_type) DO UPDATE
SET embedding  = EXCLUDED.embedding,
    model_name = EXCLUDED.model_name,
    created_at = timezone('utc', now());

-- -----------------------------------------------------------------------------
-- F3) Look photo vector search (visual nearest neighbors)
-- -----------------------------------------------------------------------------
-- Inputs: :query_vector, :limit
SELECT
    lp.id,
    lp.image_url,
    lp.outfit_id,
    lp.created_at,
    1 - (lpe.embedding <=> :query_vector::vector(512)) AS cosine_similarity
FROM ww_look_photo_embeddings AS lpe
JOIN ww_look_photos AS lp ON lp.id = lpe.look_photo_id
ORDER BY lpe.embedding <=> :query_vector::vector(512)
LIMIT :limit;

-- -----------------------------------------------------------------------------
-- Optional validation queries
-- -----------------------------------------------------------------------------
-- SELECT table_name FROM information_schema.tables WHERE table_name LIKE 'ww_%' ORDER BY table_name;
-- SELECT indexname FROM pg_indexes WHERE tablename LIKE 'ww_%' ORDER BY tablename, indexname;
*/

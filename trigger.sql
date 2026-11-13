CREATE OR REPLACE FUNCTION reject_audit_insert() RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Controlled failure: AuditLog insertion blocked';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER test_reject_audit
BEFORE INSERT ON "AuditLogs"
FOR EACH ROW EXECUTE FUNCTION reject_audit_insert();
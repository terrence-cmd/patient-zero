# Provisioned AWS Targets

Tracks who has a live AWS hosting target (via `scripts/provision-aws-target.ps1`)
so it's obvious what already exists before provisioning another one. Does **not**
contain any credentials — access keys are generated once at provisioning time and
handed to that person directly, never recorded here or anywhere else in this repo.

| Person | Bucket | CloudFront Distribution | Play URL | IAM Access | Provisioned |
|---|---|---|---|---|---|
| kenshi | `patient-zero-webgl-kenshi-493168378006` | `E1ED294MQFZ8Y7` | https://d22jn5ymxt1ztg.cloudfront.net | Attached to `BuckhornPowerwashOwner` (owner's own target) | 2026-08-10 |
| JohhnyCage | `patient-zero-webgl-johhnycage-493168378006` | `EP4FWA7E7YCQM` | https://d32acs7rpuqusf.cloudfront.net | Dedicated IAM user `JohhnyCage`, deploy-only policy `patient-zero-webgl-deploy-JohhnyCage` | 2026-08-10 |

## Adding a new target

```
.\scripts\provision-aws-target.ps1 -PersonName "<name>"
```

If the target is for someone other than the repo owner, also create them a
dedicated IAM user rather than attaching their deploy policy to the owner's
account:

```
aws iam create-user --user-name <name>
aws iam attach-user-policy --user-name <name> --policy-arn <deploy-policy-arn-from-script-output>
aws iam create-access-key --user-name <name>
```

The access key is shown once by AWS and cannot be retrieved again — hand it to
that person immediately through a secure channel (not plaintext chat/email),
then add a row to the table above.

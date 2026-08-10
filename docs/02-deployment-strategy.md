# Deployment Strategy

## The three options considered

| Option | Monthly cost | Performance | Verdict |
|---|---|---|---|
| **WebGL on AWS (S3 + CloudFront)** | ~$0 (shared account-wide free tier) | Moderate — browser sandbox, GC stalls possible | **Chosen for now** |
| **Desktop build (local .exe)** | $0 | High — full native CPU/GPU | Fallback / actual dev target once frame-timing matters |
| **Cloud streaming (GPU EC2)** | $150–$2,000+/month, hours-based | Highest, but adds streaming latency | **Rejected** |

## Why WebGL + AWS first

The explicit sequencing principle: start lightweight and free, then go
big and expensive only once it's earned. WebGL hosted on S3 + CloudFront
gets a genuinely AWS-hosted, play-from-anywhere build for close to $0/month
— both CloudFront's always-free tier (1 TB transfer, 10M requests/month)
and S3's free tier (5 GB storage) are **account-wide pools**, not
per-site. Hosting 5 separate WebGL builds costs about the same as hosting
1, because they all draw from the same shared pool.

## Why cloud GPU streaming was rejected — and not just on cost

This is the more important finding: for a *competitive fighting game*
specifically, GPU cloud streaming isn't just expensive, it's actively
counterproductive. A fighter's feel lives in tight input-to-action
latency. Streaming adds round-trip latency by design — the opposite of
what the genre needs. This ruled the option out on more than cost grounds.

## Where WebGL itself hits its ceiling

WebGL is fine for early prototyping and sharing, but browser GC stalls and
less predictable frame timing make it unsuitable for real fighting-game
development once frame-perfect combat systems (frame data, hitstun,
combos) are in play — see [00-overview.md](00-overview.md), Gate 1. From that point on,
**Desktop becomes the actual dev/test target**, and WebGL demotes to a
"share for feedback" convenience build, not where development happens.

## IAM has nothing to do with players

A separate but related question came up: does having multiple people play
a hosted game at once require separate IAM accounts? No — IAM is for
managing *AWS itself* (deploy credentials, console access), not for
players. Anyone visiting a CloudFront URL is just an anonymous web
request, the same as visiting any public website. No per-player account
of any kind is needed, at any scale relevant to this project.

## What a real online future looks like (Gate 3+)

When rollback netcode eventually enters the picture, the AWS need is a
**lightweight matchmaking/relay backend** (small EC2 or Lambda + a
database) — not GPU compute, not streaming. This is explicitly deferred
to Gate 3, and is a different, smaller kind of AWS spend than the
streaming path that was rejected here.

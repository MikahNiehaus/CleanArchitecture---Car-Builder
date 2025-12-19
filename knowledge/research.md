# Web Research Best Practices

TRIGGER: research, search, investigate, find out, look up, verify, citation, source

## Overview

This guide provides systematic methodology for conducting reliable web research that minimizes hallucinations, maximizes accuracy, and produces verifiable, well-cited findings.

## Core Principles

### 1. Verify Before Claiming
Never present unverified information as fact. Every factual claim must have a source that was actually accessed and read.

### 2. Multi-Source Verification
Cross-reference important claims across multiple authoritative sources. A single source is not verification—look for consensus.

### 3. Track Confidence
Explicitly rate confidence in findings (High/Medium/Low) based on evidence quality and source agreement.

### 4. Admit Uncertainty
Saying "I couldn't verify this" or "sources conflict on this" is better than guessing. Uncertainty acknowledgment increases credibility.

### 5. Structured Methodology
Follow a systematic process: Plan → Execute → Verify → Synthesize. Don't jump to conclusions.

---

## Research Methodology

### Phase 1: Planning

Before searching, structure the research:

1. **Decompose the Question**
   - Break complex questions into sub-questions
   - Identify what success looks like
   - Example: "How do I implement caching?" becomes:
     - What caching strategies exist?
     - What are the trade-offs?
     - What does my tech stack support?

2. **Identify Source Types**
   - Official documentation
   - Academic/research papers
   - Industry blogs and tutorials
   - Community discussions
   - News and announcements

3. **Form Initial Hypotheses**
   - Develop competing explanations
   - Track which hypothesis evidence supports
   - Stay open to unexpected findings

### Phase 2: Execution

Systematic information gathering:

1. **Targeted Search Queries**
   - Use specific, focused queries
   - Include key terms and context
   - Try multiple query variations
   - Bad: "caching"
   - Good: "Redis caching patterns Node.js 2024"

2. **Source Evaluation (While Searching)**
   - Note domain authority
   - Check publication date
   - Identify author expertise
   - Flag potential bias

3. **Information Extraction**
   - Record exact claims with context
   - Note page URLs for citation
   - Track contradictions between sources
   - Don't paraphrase numbers—quote exactly

### Phase 3: Verification

Cross-check findings:

1. **Multi-Source Confirmation**
   - Key claims need 2+ sources
   - Check if sources cite each other (circular reference)
   - Look for independent confirmation

2. **Consensus Analysis**
   - Do sources agree or conflict?
   - What do authoritative sources say?
   - Are outlier opinions credible?

3. **Number Verification**
   - Statistics must cite original source
   - Check definition matches your context
   - Note measurement methodology

4. **Recency Check**
   - Is information current?
   - Has the situation changed?
   - When was the source last updated?

### Phase 4: Synthesis

Integrate findings:

1. **Resolve Contradictions**
   - Weight evidence by source quality
   - Acknowledge when consensus unclear
   - Note which sources disagree

2. **Rate Confidence**
   - High: Multiple authoritative sources agree
   - Medium: Some sources, reasonable quality
   - Low: Limited sources or conflicting information

3. **Identify Gaps**
   - What couldn't be found?
   - What needs further research?
   - What assumptions remain unverified?

---

## Source Credibility Framework

### Tier 1: High Authority
- Official documentation (vendor, government)
- Peer-reviewed research
- Well-established industry publications
- Recognized domain experts with track record
- Official project repositories (GitHub with activity)

**Trust Level**: High—can rely on for key claims

### Tier 2: Medium Authority
- Technical blogs from practitioners
- Conference presentations with citations
- Community wikis with sources
- Industry analyst reports
- Tutorial sites from known authors

**Trust Level**: Medium—good for understanding, verify key claims

### Tier 3: Requires Verification
- Anonymous forum posts
- Uncited blog articles
- Social media claims
- AI-generated content (including other LLM outputs)
- Outdated sources (>2 years for fast-moving tech)

**Trust Level**: Low—never cite alone, always verify

### Red Flags
- No author attribution
- No publication date
- No citations or references
- Promotional content masquerading as information
- Extraordinary claims without evidence

---

## Citation Best Practices

### What to Cite
- All factual claims
- Statistics and numbers
- Direct quotes
- Technical specifications
- Methodologies from external sources

### Citation Format
```
[Claim description]
Source: [Author/Organization], "[Title]", [URL], accessed [Date]
```

### Citation Quality Checks
- [ ] Link actually works
- [ ] Content matches your claim
- [ ] Source is the original (not secondary)
- [ ] Date of publication noted
- [ ] Author/organization identified

---

## Anti-Hallucination Techniques

### Explicit Permission for Uncertainty
Give yourself permission to say:
- "I couldn't verify this"
- "Sources conflict on this point"
- "This information may be outdated"
- "I found limited information about this"

### Grounding in Evidence
- Only claim what sources explicitly state
- Don't extrapolate beyond the evidence
- Don't assume source A's claim applies to context B
- Quote exactly when precision matters

### Confidence Calibration
| Confidence | Criteria | Language |
|------------|----------|----------|
| High | 3+ quality sources agree | "X is..." |
| Medium | 1-2 sources, reputable | "According to [source], X..." |
| Low | Limited/conflicting | "Some sources suggest X, but..." |
| Uncertain | No clear evidence | "I couldn't find reliable information on..." |

### Number Verification Protocol
Numbers are common hallucination points:
1. Always cite the exact source
2. Check the number's definition/context
3. Note when the number was measured
4. Flag if you couldn't verify independently

---

## Research Report Structure

### Executive Summary
- 2-3 sentences of key findings
- Overall confidence level
- Major caveats or limitations

### Methodology
- How research was conducted
- Sources consulted
- Verification approach

### Findings
For each finding:
- Clear statement of the finding
- Confidence level (High/Medium/Low)
- Supporting sources with citations
- How it was verified

### Source Analysis Table
| Source | Type | Authority | Date | Agrees/Conflicts |
|--------|------|-----------|------|------------------|

### Uncertainties and Gaps
- What couldn't be determined
- Conflicting information
- Assumptions made
- Recommended follow-up

### Full Citations
Complete list with URLs

---

## Common Research Failures

### Search Failures
| Failure | Cause | Prevention |
|---------|-------|------------|
| Overly broad results | Generic query | Use specific terms + context |
| Missing key sources | Only one query | Try multiple query variations |
| First-result bias | Stopped too early | Check multiple sources |

### Verification Failures
| Failure | Cause | Prevention |
|---------|-------|------------|
| Wrong numbers | Different definition | Check measurement context |
| Outdated info | Didn't check date | Always note publication date |
| Circular reference | Sources cite each other | Trace to original source |

### Synthesis Failures
| Failure | Cause | Prevention |
|---------|-------|------------|
| Cherry-picking | Confirmation bias | Represent all viewpoints |
| Over-confidence | Single strong source | Require multi-source |
| Missed nuance | Rushed synthesis | Note contradictions |

---

## Research Checklist

### Before Starting
- [ ] Research question clearly defined
- [ ] Sub-questions identified
- [ ] Source types to consult listed
- [ ] Success criteria defined

### During Research
- [ ] Multiple search queries used
- [ ] Source authority evaluated
- [ ] Contradictions noted
- [ ] URLs saved for citation

### Verification
- [ ] Key claims cross-referenced (2+ sources)
- [ ] Numbers verified against original
- [ ] Publication dates checked
- [ ] Consensus/conflict documented

### Final Report
- [ ] All claims have citations
- [ ] Confidence levels assigned
- [ ] Uncertainties acknowledged
- [ ] Gaps identified
- [ ] Citations are accessible URLs

---

## Integration with Multi-Agent System

### When Research Agent Should Be Spawned
- Topic requires current information
- Technical question needs multiple perspectives
- Claim needs verification
- Unknown territory needing exploration

### Research Agent Handoffs
- **To architect-agent**: Technical findings for design
- **To estimator-agent**: Complexity research
- **To workflow-agent**: Implementation findings
- **From any agent**: "Need research on X"

### Context Usage
Store research findings in `workspace/[task-id]/context.md`:
```markdown
### research-agent - [Timestamp]
- **Task**: Research [topic]
- **Status**: COMPLETE
- **Key Findings**: [Summary]
- **Confidence**: [High/Medium/Low]
- **Handoff Notes**: [What next agent needs]
```

---

## References

This guide synthesizes best practices from:
- Anthropic Claude documentation on agentic search
- GPT Researcher architecture patterns
- Academic research on LLM fact-checking
- RAG and citation grounding techniques
- Multi-agent research system designs

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <!-- Deletes all the native lords except for the ids listed below -->

    <xsl:template match="NPCCharacter[
        @id='main_hero' or
        @id='dead_lord_6_1' or
        @id='lord_1_1' or
        @id='lord_1_7' or
        @id='lord_1_14' or
        @id='lord_2_1' or
        @id='lord_3_1' or
        @id='lord_4_1' or
        @id='lord_5_1' or
        @id='lord_6_1']" >
        <xsl:copy-of select="." />
    </xsl:template>

    <!-- <xsl:template match="NPCCharacter" /> -->
</xsl:stylesheet>
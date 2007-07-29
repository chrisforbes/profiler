<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template match="/">
		<html>
			<head>
				<title>Start Page</title>
				<link href="mru.css" type="text/css" rel="stylesheet"/>
			</head>
			<body>
				<h2>
					<span class="x" >IJW Profiler</span>
					<span class="y">
						<script>
							<![CDATA[
						document.write( window.external.GetVersion() );
						]]>
						</script>
					</span>
				</h2>
				<div>
					<div>
						<a href="#" onclick="javascript:window.external.Run('','','')">Profile an application...</a>
						<br/>
						If you have profiled some application recently, select it from the list below:
					</div>
					<div style="margin-left:40px">
						<ul>
							<xsl:for-each select="/mru/run">
								<li>
									<a href="#">
										<xsl:attribute name="onclick">
											javascript:window.external.Run(
											'<xsl:value-of select="./cmd"/>',
											'<xsl:value-of select="./dir"/>',
											'<xsl:value-of select="./args"/>');
										</xsl:attribute>
										<xsl:value-of select="./cmd"/>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</div>
					<div style="margin-top:30px">
						<a href="#" onclick="javascript:window.external.Snapshot('')">
							Load snapshot from a previous profiling run...
						</a>
						<br/>
						If you have viewed the snapshot recently, select it from the list below:
					</div>
					<div style="margin-left:40px">
						<ul>
							<xsl:for-each select="/mru/snapshot">
								<li>
									<a href="#">
										<xsl:attribute name="onclick">
											javascript:window.external.Snapshot('<xsl:value-of select="./path"/>');
										</xsl:attribute>
										<xsl:value-of select="./title"/>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>